using System;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Helper;

namespace Shunxi.Business.Protocols
{
    public interface ISerial
    {
        SerialPortStatus Status { get; set; }

        event Action<byte[]> ReceiveHandler;

        Task Open(SerialEnum serialType);
        void Send(byte[] buffer, CancellationToken token);
        
        void Cancel();
        void Close();
    }

    public enum SerialPortStatus
    {
        Opened, Opening, Closed, None, Initialled
    }
}
