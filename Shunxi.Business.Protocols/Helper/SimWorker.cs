using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.SimDirectives;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.Business.Protocols.Helper
{
    public class SimWorker
    {
        public static readonly SimWorker Instance = new SimWorker();
        private ISerial _serial;
        private BaseSimDirective _lastCommand;
        private string _receiveCache = "";
        private readonly ConcurrentQueue<CompositeDirective> _waitForSendDirectives;
        protected TaskCompletionSource<SimDirectiveResult> CmdEvent;

        private SimWorker()
        {
            _serial = new UsbSerial();
            _serial.ReceiveHandler += SerialPort_ReceiveHandler;
            _waitForSendDirectives = new ConcurrentQueue<CompositeDirective>();

            Init().IgnorCompletion();
        }

        public void GetLocation()
        {
            Enqueue(new LocationCompositeDirective(x =>
            {
                var cnetScans = x.Code as CnetScan;
                if (cnetScans == null) return;
                var url =
                    $"http://{Config.SERVER_ADDR}:{Config.SERVER_PORT}/api/sim/location?mcc={cnetScans.MCC}&mnc={cnetScans.MNC}" +
                    $"&lac={cnetScans.Lac}&ci={cnetScans.Cellid}&deviceid={Common.Utility.Common.GetUniqueId()}";
                Enqueue(new HttpCompositeDirective(url, p =>
                {
                    LogFactory.Create().Info("get location " + p.Status);
                }));
            }));
        }

        private async Task Init()
        {
            while (true)
            {
                CompositeDirective temp = null;
                if (_waitForSendDirectives.TryDequeue(out temp))
                {
                    if (temp == null)
                    {
                        await Task.Delay(10);
                        continue;
                    }
                    if (temp.DirectiveType == SimDirectiveType.Location)
                    {
                        await SendCommand(new CengWriteDirective());
                        var x = await SendCommand(new CengReadDirective());

                        temp.Handle(x).IgnorCompletion();
                    }
                    else if (temp.DirectiveType == SimDirectiveType.HttpGet)
                    {
                        var directive = temp as HttpCompositeDirective;
                        if (directive == null)
                        {
                            await Task.Delay(10);
                            continue;
                        }
                        await SendCommand(new CregDirective());
                        var x = await SendCommand(new HttpBearerQueryDirective());
                        if (!x.Status) //bearer错误 需要关闭后在打开
                        {
                            await SendCommand(new HttpBearerCloseDirective());
                            await SendCommand(new HttpBearerOpenDirective());
                        }
                        else if (!x.IsExecOk)
                        {
                            await SendCommand(new HttpBearerOpenDirective());
                        }
                        await SendCommand(new HttpInitDirective());
                        await SendCommand(new HttpParaUrlDirective(directive.Url));
                        await SendCommand(new HttpParaCidDirective());
                        await SendCommand(new HttpActionGetDirective());

                        var p = await SendCommand(new HttpReadDirective());
                        await SendCommand(new HttpTermDirective());

                        temp.Handle(p).IgnorCompletion();
                    }
                }

                await Task.Delay(10);
            }
        }

        private void SerialPort_ReceiveHandler(byte[] obj)
        {
            Debug.WriteLine("start-------------------\r\n" + Encoding.UTF8.GetString(obj) + "\r\nend-------------------");
            //需要判断是否包含服务端的回复信息 如：hello 回 world
            var str = Encoding.UTF8.GetString(obj);
            if (str.IndexOf("AT+", StringComparison.Ordinal) == 0)
            {
                _receiveCache = str;
            }
            else
            {
                _receiveCache += str;
            }

            if (_lastCommand == null)
            {
                Debug.WriteLine("LastCommand is null");
                return;
            }

            Debug.WriteLine("LastCommand->" + _lastCommand.DirectiveText);
            if (_lastCommand?.isEnd(_receiveCache) ?? false)
            {
                if (!CmdEvent.Task.IsCompleted && !CmdEvent.Task.IsCanceled)
                {
                    CmdEvent.TrySetResult(_lastCommand?.Process(_receiveCache));
                }
            }
        }

        public void Enqueue(CompositeDirective directive)
        {
            if (_waitForSendDirectives.Count >= 500) return;
            LogFactory.Create().Info("new directive " + directive.GetType());
            _waitForSendDirectives.Enqueue(directive);
        }

        public async Task SendData(string msg)
        {
            var x = Encoding.UTF8.GetBytes(msg);
            var p = new CancellationTokenSource();
            // 第一次加载
            if (_serial.Status == SerialPortStatus.Initialled)
            {
                await _serial.Open(SerialEnum.Sim);
            }

            if (_serial.Status == SerialPortStatus.Opened)
                 _serial.Send(x.Concat(new byte[] { 0x0D, 0x0A }).ToArray(), p.Token);
        }

        //发送at指令
        public async Task<SimDirectiveResult> SendCommand(BaseSimDirective cmd, int timeout = 5000)
        {
            await Task.Delay(5);//防止指令发送太密集
            CmdEvent = new TaskCompletionSource<SimDirectiveResult>();
            _lastCommand = cmd;
            await SendData(cmd.DirectiveText);
            var p = CmdEvent.Task;

            var cancellationToken = new CancellationTokenSource(timeout).Token;
            var cancellationCompletionSource = new TaskCompletionSource<SimDirectiveResult>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(new SimDirectiveResult(false, "timeout"))))
            {
                var t = await Task.WhenAny(p, cancellationCompletionSource.Task);
                if (p != t)
                {
                    CmdEvent.TrySetResult(new SimDirectiveResult(false, "timeout"));
                }
                _receiveCache = "";
                _lastCommand = null;
                return t.Result;
            }

        }
    }
}
