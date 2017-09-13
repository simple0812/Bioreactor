using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Infrastructure.Common.Extension;
using Shunxi.Infrastructure.Common.Log;
using Shunxi.Infrastructure.Common.Utility;

namespace Shunxi.Business.Protocols
{
    public class NetSerial: ISerial
    {
        private readonly StreamSocket client = null;
        private bool isConnect => null != client;
        private static object _locker = new object();
        private const string PORT = "3084";
        private const string ADDR = "100.0.0.54";
        public SerialPortStatus Status { get; set; }
        CancellationTokenSource _readCancellationTokenSource;
        public event Action<byte[]> ReceiveHandler;
        private DataReader reader;

        public NetSerial()
        {
            client = new StreamSocket();
            Status = SerialPortStatus.Initialled;
        }

        public async Task Open(SerialEnum serialType)
        {
            if (Status == SerialPortStatus.Initialled)
            {
                try
                {
                    var serverHost = new HostName(ADDR);
                    Status = SerialPortStatus.Opening;
                    await client.ConnectAsync(serverHost, PORT).AsTask(new CancellationTokenSource(1000).Token);
                    Status = SerialPortStatus.Opened;

                    _readCancellationTokenSource = new CancellationTokenSource();
                    Listen().IgnorCompletion();
                }
                catch (Exception e)
                {
                    LogFactory.Create().Info("net serial open error" + e.Message);
                    Status = SerialPortStatus.Initialled;
                }
            }
        }

        public async Task Listen()
        {
            try
            {
                reader = new DataReader(client.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;

                while (!_readCancellationTokenSource.IsCancellationRequested)
                {
                    var len = await reader.LoadAsync(1024);  //获取一定大小的数据流  
                    var xdata = new byte[len];
                    reader.ReadBytes(xdata);
                    reader.DetachBuffer();

                    LogFactory.Create().Info($"receive ->{Common.BytesToString(xdata)}<- receive end");
                    OnReceiveHandler(xdata);

                    await Task.Delay(5, _readCancellationTokenSource.Token);
                }
            }
            catch (TaskCanceledException)
            {
                LogFactory.Create().Info("net serial listen cancel");
                Status = SerialPortStatus.Initialled;
            }
            catch (Exception ex)
            {
                LogFactory.Create().Info("net serial listen error" + ex.Message);
                Status = SerialPortStatus.Initialled;
            }
            finally
            {
                if (reader != null)
                {
                    reader.DetachBuffer();
                    reader.DetachStream();
                    reader.Dispose();
                }
            }
        }

        public async Task Send(byte[] data, CancellationToken token)
        {
            try
            {
                DataWriter writer = new DataWriter(client.OutputStream);  
                writer.WriteBytes(data);  
                await writer.StoreAsync().AsTask(token);
                LogFactory.Create().Info($"send ->{Common.BytesToString(data.ToArray())}<- send end");
                writer.DetachStream();  
                writer.Dispose();  
            }
            catch (Exception ex)
            {
                Status = SerialPortStatus.Initialled;
                LogFactory.Create().Info("serial Send error:" + ex.Message);
            }
        }

        public void Cancel()
        {
            if (_readCancellationTokenSource != null)
            {
                if (!_readCancellationTokenSource.IsCancellationRequested)
                {
                    _readCancellationTokenSource.Cancel();
                }
            }
            Status = SerialPortStatus.Closed;
        }

        public void Close()
        {
            try
            {
                Cancel();
                client?.Dispose();
                Status = SerialPortStatus.None;
                LogFactory.Create().Info("serialport dispose");
            }
            catch (Exception e)
            {
                LogFactory.Create().Info("serialport close error ->" + e.Message);
            }
        }

        protected virtual void OnReceiveHandler(byte[] obj)
        {
            ReceiveHandler?.Invoke(obj);
        }
    }
}
