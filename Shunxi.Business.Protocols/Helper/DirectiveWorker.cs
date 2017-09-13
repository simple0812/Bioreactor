using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.Business.Protocols.Helper
{
    public sealed class DirectiveWorker : IDisposable
    {
        private IList<WaitForSend> _waitForSendDirectives;
        private List<byte> _dirtyDirective;
        private ConcurrentDictionary<int, WaitForFeedBack> _waitForFeedbackDirectives;
        private CancellationTokenSource _cancelTokenSource;

        private readonly IProtocol _protocolProvider = ProtocolFactory.Create(ProtocolVersion.V485_1);

        private static readonly object Locker = new object();
        private static readonly object ParseLocker = new object();
        private bool _isRetry = true;
        private ISerial _serialPort = new UsbSerial();

        private DirectiveWorker()
        {
            Init();
        }

        public void Init()
        {
            _serialPort.ReceiveHandler += SpHelper_ReceiveHandler;

            _waitForSendDirectives = new List<WaitForSend>();
            _dirtyDirective = new List<byte>();
            _waitForFeedbackDirectives = new ConcurrentDictionary<int, WaitForFeedBack>();

            _cancelTokenSource = new CancellationTokenSource();

            Task.Run(() =>
            {
                 DispatchDirective(_cancelTokenSource.Token).IgnorCompletion();
            });

            Task.Run(() =>
            {
                RetrySend(_cancelTokenSource.Token).IgnorCompletion(); ;
            });
        }

        private void SpHelper_ReceiveHandler(byte[] data)
        {
            ParseResultAndNotify(data);
        }

        public void SetIsRtry(bool retry)
        {
            _isRetry = retry;
        }

        private static volatile DirectiveWorker _instance = null;
        private static readonly object InstanceLocker = new object();
        public static DirectiveWorker Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLocker)
                    {
                        if (_instance == null)
                            _instance = new DirectiveWorker();
                    }
                }
                return _instance;
            }
        }

        public delegate void SerialPortEventHandler(SerialPortEventArgs args);
        public event SerialPortEventHandler SerialPortEvent;
        public event Action<CustomException, BaseDirective> ErrorEvent;

        public void OnErrorEvent(CustomException args, BaseDirective directive)
        {
            ErrorEvent?.Invoke(args, directive);
            Dispose();
        }

        public void OnSerialPortEvent(SerialPortEventArgs args)
        {
            SerialPortEvent?.Invoke(args);
        }

        //指令排序：优先按照超时时间多少排列(比如大于3秒为超时，然后根据超过的时间量排序) 然后按照指令优先级排列
        public void PrepareDirective(BaseDirective item, int reSendTimes = 0)
        {
            lock (Locker)
            {
                if (_waitForSendDirectives.Count <= 100)
                    _waitForSendDirectives.Add(new WaitForSend(DateTime.Now, item, reSendTimes));
            }
        }

        private async Task DispatchDirective(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();

                while (!_cancelTokenSource.IsCancellationRequested)
                {
                    WaitForSend tuple = null;
                    lock (Locker)
                    {
                        var now = DateTime.Now;
                        if (_waitForFeedbackDirectives.Count > 0) continue;

                        tuple =
                            _waitForSendDirectives.OrderByDescending(p => p.RetrySendTimes)
                                .ThenByDescending(p =>
                                {
                                    var span = (now - p.CreatedAt).TotalMilliseconds;
                                    return span > 1000 ? span : 0;
                                })
                                .ThenBy(p => p.Directive.Priority)
                                .FirstOrDefault();

                        if (tuple != null)
                            _waitForSendDirectives.Remove(tuple);
                    }

                    if (tuple == null)
                    {
                        await Task.Delay(50, token);
                        continue;
                    }

                    var item = tuple.Directive;
                    try
                    {
                        await Send(item, tuple.RetrySendTimes);
                    }
                    catch (CustomException ce)
                    {
                        OnErrorEvent(ce, item);
                    }
                    catch (TaskCanceledException)
                    {
                        LogFactory.Create().Info("feedbackSource cancel");
                        OnErrorEvent(new CustomException($"{Config.RETRY_TIMES}重试发送指令失败", this.GetType().FullName,
                            ExceptionPriority.Unrecoverable), item);
                        return;
                    }
                    catch (Exception ex)
                    {
                        OnErrorEvent(new CustomException(ex.Message, this.GetType().FullName,
                            ExceptionPriority.Unrecoverable), item);
                        return;
                    }

                    await Task.Delay(50, token);
                }
            }
            catch (TaskCanceledException)
            {
                LogFactory.Create().Info("DispatchDirective cancel");
            }
        }

        public async Task Send(BaseDirective item, int reSendTimes = 0)
        {
            try
            {
                _waitForFeedbackDirectives.TryAdd(item.DirectiveId, new WaitForFeedBack(DateTime.Now, item, reSendTimes));

                var directiveData = _protocolProvider.GenerateDirectiveBuffer(item);
                //断线重连
                if (_serialPort.Status == SerialPortStatus.Initialled)
                {
                    await _serialPort.Open(SerialEnum.LowerComputer);
                }

                if (_waitForFeedbackDirectives.ContainsKey(item.DirectiveId) && _serialPort.Status == SerialPortStatus.Opened)
                    _serialPort.Send(directiveData, _cancelTokenSource.Token);
            }
            catch (CustomException)
            {
                LogFactory.Create().Info("send error");
            }
            catch (TaskCanceledException)
            {
                LogFactory.Create().Info("send cancel");
            }
            catch (Exception e)
            {
                OnErrorEvent(new CustomException(e.Message + "Send", this.GetType().FullName,
                    ExceptionPriority.Unrecoverable), item);
            }
        }

        private void HandleFeedback(DirectiveResult recvData, byte[] directiveBytes)
        {

            WaitForFeedBack metadata = null;

            if (!_waitForFeedbackDirectives.ContainsKey(recvData.Data.DirectiveId))
            {
                LogFactory.Create().Warnning($"waitForFeedbackDirectives not ContainsKey {recvData.Data.DirectiveId}");
                return;
            }

            var feedback = _waitForFeedbackDirectives[recvData.Data.DirectiveId];

            //修正同一个指令id，发送和反馈对应的设备不同或者类型不同
            if (feedback?.Directive == null || feedback.Directive.TargetDeviceId != recvData.Data.DeviceId)
            {
                LogFactory.Create().Warnning("send and feedback not match");
                return;
            }

            if (_waitForFeedbackDirectives.TryRemove(recvData.Data.DirectiveId, out metadata) && null != metadata)
            {
                var args = new SerialPortEventArgs
                {
                    IsSucceed = true,
                    Result = recvData,
                    Command = directiveBytes
                };

                OnSerialPortEvent(args);
            }
            else
            {
                LogFactory.Create().Warnning($"waitForFeedbackDirectives TryRemove {recvData.Data.DirectiveId} failed");
            }

        }

        //只重发成功发送的指令
        private async Task RetrySend(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                while (!_cancelTokenSource.IsCancellationRequested)
                {
                    if (_waitForFeedbackDirectives.Count != 0)
                    {
                        var waitForFeedback = _waitForFeedbackDirectives.Values.FirstOrDefault();
                        if (waitForFeedback != null)
                        {
                            var now = DateTime.Now;
                            if ((now - waitForFeedback.CreatedAt).TotalMilliseconds >= Config.RETRY_TIMEOUT)
                            {
                                var p =
                                    Shunxi.Common.Utility.Common.BytesToString(
                                        _protocolProvider.GenerateDirectiveBuffer(waitForFeedback.Directive));


                                if (waitForFeedback.RetrySendTimes < Config.RETRY_TIMES)
                                {

                                    LogFactory.Create()
                                        .Info(
                                            $"{waitForFeedback.CreatedAt:yyyy-MM-dd HH:mm:ss.fff}<-timeout->{now:yyyy-MM-dd HH:mm:ss.fff}");

                                    WaitForFeedBack metadata = null;

                                    if (_isRetry)
                                    {
                                        LogFactory.Create()
                                            .Info(
                                                $"{_waitForFeedbackDirectives.Count}count,{waitForFeedback.RetrySendTimes}times retry ->{p}<- retry end");

                                        PrepareDirective(waitForFeedback.Directive, ++waitForFeedback.RetrySendTimes);
                                    }
                                    else
                                    {
                                        OnSerialPortEvent(new SerialPortEventArgs() { IsSucceed = false, Message = waitForFeedback.Directive.TargetDeviceId.ToString() });
                                    }

                                    _waitForFeedbackDirectives.TryRemove(waitForFeedback.Directive.DirectiveId,
                                        out metadata);
                                }
                                else
                                {
                                    LogFactory.Create()
                                        .Info(
                                            $"{_waitForFeedbackDirectives.Count}count,{p}, {waitForFeedback.RetrySendTimes}times retry failed");

                                    OnErrorEvent(new CustomException($"{Config.RETRY_TIMES}重试发送指令失败", this.GetType().FullName,
                                        ExceptionPriority.Unrecoverable), waitForFeedback.Directive);
                                    return;
                                }
                            }
                        }
                    }

                    await Task.Delay(Config.RETRY_INTERVAL, token);
                }
            }
            catch (TaskCanceledException)
            {
                LogFactory.Create().Info("RetrySend cancel");
            }
        }

        private byte[] GetRawDirective(byte[] b)
        {
            if (b.Length <= 2) return b;

            //TODO 临时补丁 下午机会在指令2端加上0x00
            if (b[0] == 0x00 || b[0] == 0xff)
            {
                b = b.Skip(1).Take(b.Length - 1).ToArray();
            }

            var len = ((DirectiveTypeEnum)b[1]).GetFeedbackLength();
            return b.Take(len).ToArray();
        }

        //解析指令，如果成功则清空DirtyData
        //如果失败则先判断是否上一次是否也解析失败 如果没有则将该次指令保存到DirtyData
        //如果有则将该次指令与DirtyData合并在一起解析 如果仍然失败 则清空DirtyData
        private void ParseResultAndNotify(byte[] b)
        {
            lock (ParseLocker)
            {
                var recvData = _protocolProvider.ResolveFeedback(GetRawDirective(b));

                if (null == recvData || !recvData.Status)
                {
                    if (_dirtyDirective.Any())
                    {
                        _dirtyDirective.AddRange(b);
                        var newBytes = _dirtyDirective.ToArray();
                        var newData = _protocolProvider.ResolveFeedback(GetRawDirective(newBytes));
                        if (null == newData || !newData.Status)
                        {
                            //如果DirtyDirective不是有效的指令则清空
                            if (_dirtyDirective.Count > 2)
                            {
                                var len = ((DirectiveTypeEnum)_dirtyDirective[1]).GetFeedbackLength();

                                if (len == 0 || len <= _dirtyDirective.Count)
                                {
                                    _dirtyDirective.Clear();
                                    LogFactory.Create().Info(".....recvData is error.....");
                                    _dirtyDirective.AddRange(b);
                                }
                            }
                        }
                        else
                        {
                            _dirtyDirective.Clear();
                            HandleFeedback(newData, newBytes);
                        }
                    }
                    else
                    {
                        _dirtyDirective.AddRange(b);
                    }
                }
                else
                {
                    _dirtyDirective.Clear();
                    HandleFeedback(recvData, b);
                }
            }
        }

        public void Cancel()
        {
            if (_cancelTokenSource != null)
            {
                if (!_cancelTokenSource.IsCancellationRequested)
                {
                    _cancelTokenSource.Cancel();
                }
            }

            if (_serialPort != null)
            {
                _serialPort.ReceiveHandler -= SpHelper_ReceiveHandler;
                _serialPort = null;
            }

            _waitForFeedbackDirectives.Clear();
        }

        public void Dispose()
        {
            Cancel();
            _instance = null;
        }

        public void Clean()
        {
            lock (Locker)
            {
                _waitForSendDirectives.Clear();
            }

            lock (ParseLocker)
            {
                _dirtyDirective.Clear();
            }

            _waitForFeedbackDirectives.Clear();
        }
    }

    public class SerialPortEventArgs : EventArgs
    {
        public bool IsSucceed { get; set; }
        public DirectiveResult Result { get; set; }
        public string Message { get; set; }
        public byte[] Command { get; set; }
    }

    public class WaitForFeedBack
    {
        public DateTime CreatedAt { get; set; }
        public BaseDirective Directive { get; set; }
        public bool IsSendSuccess { get; set; } = false;
        public int RetrySendTimes { get; set; } = 0;

        public WaitForFeedBack(DateTime time, BaseDirective directive, int reSendTimes)
        {
            CreatedAt = time;
            Directive = directive;
            RetrySendTimes = reSendTimes;
        }
    }

    public class WaitForSend
    {
        public DateTime CreatedAt { get; set; }
        public BaseDirective Directive { get; set; }
        public double TimeOut { get; set; } = 0;
        public int RetrySendTimes { get; set; } = 0;

        public WaitForSend(DateTime time, BaseDirective directive, int reSendTimes = 0)
        {
            CreatedAt = time;
            Directive = directive;
            RetrySendTimes = reSendTimes;
        }
    }
}
