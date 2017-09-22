using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Extension;
using SuperSocket.ProtoBase;

namespace Shunxi.Business.Protocols
{
    public class NetSerial: ISerial
    {
        private readonly SocketClient client = null;
        private bool isConnect => null != client;
        private static object _locker = new object();
        private const int PORT = 3084;
        private const string ADDR = "10.0.0.44";
        public SerialPortStatus Status { get; set; }
        CancellationTokenSource _readCancellationTokenSource;
        public event Action<byte[]> ReceiveHandler;

        public NetSerial()
        {
            client = new SocketClient();

            Status = SerialPortStatus.Initialled;
        }

        public async Task Open(SerialEnum serialType)
        {
            await Task.Yield();
            if (Status == SerialPortStatus.Initialled)
            {
                try
                {
                    Status = SerialPortStatus.Opening;
                    client.Connect(ADDR, PORT);

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
            await Task.Yield();
        }

        public void Send(byte[] data, CancellationToken token)
        {
            try
            {
                client.Send(data);
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


    public class SocketClient
    {
        private Socket newclient; //= new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        private readonly byte[] buffer = new byte[1024];

        ~SocketClient()
        {
            Close();
        }

        public bool IsIP(string ip)
        {
            return Regex.IsMatch(ip, @"^((2[0-4]\d|25[0-5]|[01]?\d\d?)\.){3}(2[0-4]\d|25[0-5]|[01]?\d\d?)$");
        }

        public void ByteMencpy(byte[] bufferDart, byte[] bufferOrig, int lend)
        {
            for (int i = 0; i < lend; i++)
            {
                bufferDart[i] = bufferOrig[i];
            }
        }

        public bool Connect(string ipString, int port)
        {
            var ie = IsIP(ipString) 
                ? new IPEndPoint(IPAddress.Parse(ipString), port) 
                : new IPEndPoint(Dns.GetHostEntry(ipString).AddressList[0], port);

            try
            {
                newclient = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                newclient.Connect(ie);
                newclient.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), newclient);
            }
            catch (SocketException e)
            {
                LogFactory.Create().Error("unable to connect to server " + e.Message) ;
                return false;
            }

            return true;
        }

        void ReceiveCallback(IAsyncResult result)
        {
            try
            {
                Socket ts = (Socket) result.AsyncState;
                int len = ts.EndReceive(result);
                result.AsyncWaitHandle.Close();
                if (len > 0)
                {
                    byte[] bufferTemp = new byte[len];
                    ByteMencpy(bufferTemp, buffer, len);
                    LogFactory.Create().Info("receive ->" + Common.Utility.Common.BytesToString(bufferTemp) + "<- receive end");
                }

                ts.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), ts);
            }
            catch (SocketException e)
            {
                LogFactory.Create().Info("接收异常:" + e.Message);
            }
        }

        public bool Send(string sendStr)
        {
            byte[] bs = Encoding.ASCII.GetBytes(sendStr);
            LogFactory.Create().Info("send ->" + Encoding.ASCII.GetString(bs) + "<- send end");
            newclient.Send(bs);
            return true;
        }

        public bool Send(byte[] bs)
        {
            int len = -1;
            try
            {
                LogFactory.Create().Info("send ->" + Common.Utility.Common.BytesToString(bs) + "<- send end");
                len = newclient.Send(bs);
            }
            catch (Exception)
            {
                //
            }

            return len > 0;
        }

        public int Receive(byte[] recvBytes)
        {
            var bytes = newclient.Receive(recvBytes, recvBytes.Length, 0);
            return bytes;
        }

        public bool Close()
        {
            try
            {
                newclient.Shutdown(SocketShutdown.Both);
                newclient.Close();
            }
            catch (Exception)
            {
                //
            }
            return true;
        }
    }
}
