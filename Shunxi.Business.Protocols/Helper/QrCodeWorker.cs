using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.SimDirectives;

namespace Shunxi.Business.Protocols.Helper
{
    //需要将二维码扫描枪的接口模式设置为串口模式 详情见扫描枪的说明书
    public class QrCodeWorker
    {
        public static readonly QrCodeWorker Instance = new QrCodeWorker();
        protected TaskCompletionSource<SimDirectiveResult> CmdEvent;
        public readonly UsbSerial Serial = new UsbSerial();

        private QrCodeWorker()
        {
        }

        private void serial_ReceiveHandler(byte[] obj)
        {
        }

        public async Task Init()
        {
            await Task.Yield();
            if (Serial.Status == SerialPortStatus.Initialled)
            {
                await Serial.Open("Barcode Reader");
            }
        }
    }
}
