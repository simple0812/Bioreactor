using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.Business.Protocols
{
   
    public sealed class UsbSerial : ISerial
    {
        //缓存
        private static readonly Dictionary<SerialEnum, SerialPort> _serialHelpers = new Dictionary<SerialEnum, SerialPort>();
        public SerialPortStatus Status { get; set; }
        public SerialPort SerialPort = null;
        CancellationTokenSource _readCancellationTokenSource;
        private TaskCompletionSource<byte[]> completionSource;
        public event Action<byte[]> ReceiveHandler;

        public const int WRITE_TIMEOUT = 50;
        public const int READ_TIMEOUT = 50;

        public UsbSerial()
        {
            Status = SerialPortStatus.Initialled;
        }

        public UsbSerial(SerialPort device)
        {
            Status = SerialPortStatus.Initialled;
            SerialPort = device;
        }

        private byte[] generatePingBytes(byte id)
        {
            var p = new byte[] { id, 0x03, 0xff, 0xfe, 0x00 };
            var t = DirectiveHelper.GenerateCheckCode(p);
            return p.Concat(t).ToArray();
        }

        private byte[] generatePingBytesByType(SerialEnum serialType)
        {
            byte[] ret = {};
            switch (serialType)
            {
                case SerialEnum.LowerComputer:
                    ret = generatePingBytes(Config.DetectorId);
                        break;
                case SerialEnum.Sim:
                    ret = Encoding.UTF8.GetBytes("AT+CCID").Concat(new byte[] {0x0D, 0x0A}).ToArray();
                        break;
                default:break;
            }

            return ret;
        }

        public async Task Open(string portName)
        {
            await Task.Yield();

            if (Status == SerialPortStatus.Initialled)
            {
                try
                {
                    Status = SerialPortStatus.Opening;
                    SerialPort = new SerialPort(portName);

                    SerialPort.WriteTimeout = WRITE_TIMEOUT;
                    SerialPort.ReadTimeout = READ_TIMEOUT;
                    SerialPort.BaudRate = 9600;
                    SerialPort.Parity = Parity.None;
                    SerialPort.StopBits = StopBits.One;
                    SerialPort.DataBits = 8;
                    SerialPort.Handshake = Handshake.None;
                    SerialPort.DtrEnable = true; // 扫码枪需要打开设置该选项 否则无法通信
                    SerialPort.WriteBufferSize = 1024;
                    SerialPort.ReadBufferSize = 1024;

                    SerialPort.DataReceived += SerialPort_DataReceived;
                    SerialPort.ErrorReceived += SerialPort_ErrorReceived;

                    _readCancellationTokenSource = new CancellationTokenSource();
                    SerialPort.Open();

                    Status = SerialPortStatus.Opened;
                    LogFactory.Create().Info($"Serial Port {portName} {Status}");
                }
                catch (Exception ex)
                {
                    SerialPort.DataReceived -= SerialPort_DataReceived;
                    SerialPort.ErrorReceived -= SerialPort_ErrorReceived;
                    Status = SerialPortStatus.Initialled;
                    SerialPort?.Close();
                    SerialPort?.Dispose();
                    LogFactory.Create().Info("port open error" + ex.Message);
                }
            }
        }

        public async Task Open(SerialEnum serialType)
        {
            if (Status == SerialPortStatus.Initialled)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                Status = SerialPortStatus.Opening;
                var dis = SerialPort.GetPortNames();
                if (!dis.Any()) return;

                foreach (COMPortInfo comPort in COMPortInfo.GetCOMPortsInfo())
                {
                    Console.WriteLine($"{comPort.Name} – {comPort.Description}");
                    if(comPort.Description.IndexOf("USB", StringComparison.Ordinal) == -1
                        && comPort.Description.IndexOf("Virtual Serial Port", StringComparison.Ordinal) == -1) continue;

                    try
                    {
                        var portName = comPort.Name.Split("->".ToCharArray())[0];
                        LogFactory.Create().Info("portname:" + portName);

                        SerialPort = new SerialPort(portName);
                        if (SerialPort == null || SerialPort.IsOpen) continue;

                        SerialPort.WriteTimeout = WRITE_TIMEOUT;
                        SerialPort.ReadTimeout = READ_TIMEOUT;
                        SerialPort.BaudRate = 9600;
                        SerialPort.Parity = Parity.None;
                        SerialPort.StopBits = StopBits.One;
                        SerialPort.DataBits = 8;
                        SerialPort.Handshake = Handshake.None;
                        SerialPort.WriteBufferSize = 1024;
                        SerialPort.ReadBufferSize = 1024;

                        SerialPort.DataReceived += SerialPort_DataReceived;
                        SerialPort.ErrorReceived += SerialPort_ErrorReceived;

                        _readCancellationTokenSource = new CancellationTokenSource();
                        SerialPort.Open();
                        Send(generatePingBytesByType(serialType), CancellationToken.None);

                        var cancellationToken = new CancellationTokenSource(1000).Token;
                        completionSource = new TaskCompletionSource<byte[]>();
                        using (cancellationToken.Register(() => completionSource.TrySetResult(new byte[] { })))
                        {
                            var x = await completionSource.Task;
                            var ret = Shunxi.Common.Utility.Common.BytesToString(x);
                            if (!string.IsNullOrEmpty(ret) && ret.Length > 10)
                            {
                                Status = SerialPortStatus.Opened;
                                LogFactory.Create().Info($"Serial Port {portName} Opened");
                                break;
                            }

                            Status = SerialPortStatus.Initialled;
                            SerialPort.DataReceived -= SerialPort_DataReceived;
                            SerialPort.ErrorReceived -= SerialPort_ErrorReceived;
                            SerialPort.Close();
                            SerialPort.Dispose();
                            SerialPort = null;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (SerialPort != null)
                        {
                            SerialPort.DataReceived -= SerialPort_DataReceived;
                            SerialPort.ErrorReceived -= SerialPort_ErrorReceived;
                        }

                        Status = SerialPortStatus.Initialled;
                        SerialPort?.Close();
                        SerialPort?.Dispose();
                        LogFactory.Create().Info("port open error" + ex.Message);
                    }
                }

                sw.Stop();
                LogFactory.Create().Info("open port consume time " + sw.ElapsedMilliseconds);

            }
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var comPort = sender as SerialPort;
            if (comPort == null) return;

            var bytes = comPort.BytesToRead;
            var comBuffer = new byte[bytes];
            comPort.Read(comBuffer, 0, bytes);

            if (comBuffer.Length == 0)
            {
                return;
            }

            LogFactory.Create().Info($"receive ->{Shunxi.Common.Utility.Common.BytesToString(comBuffer)}<- receive end, {comBuffer.Length}");

            if (Status == SerialPortStatus.Opening || Status == SerialPortStatus.Initialled)
            {
                //排除噪声干扰
//                if(!(comBuffer.Length == 1 && comBuffer[0] == 0x00))
                if(comBuffer.Length >= 8)
                    completionSource?.TrySetResult(comBuffer);
            }
            else
            {
                OnReceiveHandler(comBuffer);
            }
        }

        private void SerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            LogFactory.Create().Error("SerialPort Received Error" + e.EventType);
        }

        public void Send(byte[] buffer, CancellationToken token)
        {
            //运行过程中串口异常
            try
            {
                LogFactory.Create().Info($"send ->{Shunxi.Common.Utility.Common.BytesToString(buffer)}<- send end");
                SerialPort.Write(buffer, 0, buffer.Length);
            }
            catch (Exception ex)
            {
                if (SerialPort != null)
                {
                    SerialPort.DataReceived -= SerialPort_DataReceived;
                    SerialPort.ErrorReceived -= SerialPort_ErrorReceived;
                }

                Status = SerialPortStatus.Initialled;
                SerialPort?.Close();
                SerialPort?.Dispose();
                LogFactory.Create().Error("usb serial port send msg error" + ex.Message);
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
                SerialPort?.Dispose();
                SerialPort = null;
                Status = SerialPortStatus.None;
                LogFactory.Create().Info("serialport dispose");
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("serialport close error ->" + e.Message);
            }
        }

        private void OnReceiveHandler(byte[] obj)
        {
            ReceiveHandler?.Invoke(obj);
        }
    }
}
