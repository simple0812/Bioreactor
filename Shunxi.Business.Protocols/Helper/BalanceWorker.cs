using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Common.Log;

namespace Shunxi.Business.Protocols.Helper
{
    public sealed class BalanceWorker : IDisposable
    {
        private List<byte> _dirtyDirective;


        private static readonly object Locker = new object();
        private static readonly object ParseLocker = new object();
        private bool _isRetry = true;
        public ISerial _serialPort = new UsbSerial();

        private BalanceWorker()
        {
            Init();
        }

        public void Init()
        {
            _dirtyDirective = new List<byte>();
            _serialPort.ReceiveHandler += SpHelper_ReceiveHandler;
        }

        private void SpHelper_ReceiveHandler(byte[] data)
        {
            ParseResultAndNotify(data);
        }

        public void SetIsRtry(bool retry)
        {
            _isRetry = retry;
        }

        private static volatile BalanceWorker _instance = null;
        private static readonly object InstanceLocker = new object();
        public static BalanceWorker Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLocker)
                    {
                        if (_instance == null)
                            _instance = new BalanceWorker();
                    }
                }
                return _instance;
            }
        }

        public event Action<double> SerialPortEvent;

        public void OnSerialPortEvent(double args)
        {
            SerialPortEvent?.Invoke(args);
        }

        //该方法能解析完整指令分为任意段的情况
        private void ParseResultAndNotify(byte[] b)
        {
            lock (ParseLocker)
            {
                var ret = resolveDirtyData(b);
            }
        }

        //byte[0]=0x2b byte[5]=0x2e 结尾为0x0d 0x0a
        private bool resolveDirtyData(byte[] dirtyData)
        {
            _dirtyDirective.AddRange(dirtyData);

            while (true)
            {
                if (_dirtyDirective.Count <= 6) return false;
                //指令头字节为设备编号 如果为00x00 则该指令无法解析
                if (_dirtyDirective[0] != 0x2b || _dirtyDirective[5] != 0x2e)
                {
                    _dirtyDirective.RemoveAt(0);
                    continue;
                }

                //反馈指令长度
                var len = 12;
                if (_dirtyDirective.Count >= len)
                {
                    var arr = _dirtyDirective.GetRange(0, len).ToArray();

                    if (arr[len - 2] == 0x0d && arr[len - 1] == 0x0a)
                    {
                       var p = ResolveFeedback(arr);
                        _dirtyDirective.RemoveRange(0, len);
                        OnSerialPortEvent(p);
                        return true;
                    }
                    //解析失败后 移除头字节后重新解析
                    _dirtyDirective.RemoveAt(0);
                    LogFactory.Create().Info(".....recvData is error.....");
                }
                else//一条指令被分为几段收到 前几段会被保存到_dirtyDirective里面 直到整段指令完整后一起解析
                {
                    return false;
                }
            }
        }

        public double ResolveFeedback(byte[] arr)
        {
            if (arr.Length <= 5)
            {
                return 0D;
            }

            return (arr[1] - 0x30) * 1000 + 
                (arr[2] - 0x30) * 100 + 
                (arr[3] - 0x30) * 10 + 
                (arr[4] - 0x30)  + 
                (arr[6] - 0x30) * 0.1 + 
                (arr[7] - 0x30) * 0.01;
        }

        public void Dispose()
        {
            _instance = null;
        }

        public void Clean()
        {
        }
    }
}
