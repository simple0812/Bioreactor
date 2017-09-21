using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;

namespace Shunxi.Business.Logic.Devices
{
    public class GasDevice: DeviceBase
    {
        public double Flowrate { get; set; }
        public double Concentration { get; set; }

        public GasDevice(int id = 0x91)
        {
            DeviceId = id;
            DeviceType = TargetDeviceTypeEnum.Gas;
        }

        public void SetParams(double flowrate, double concentration)
        {
            Flowrate = flowrate;
            Concentration = concentration;
        }

        protected override TryStartDirective GenerateTryStartDirective()
        {
            return new TryStartDirective(DeviceId, Flowrate, Concentration * 10, (int)DirectionEnum.Anticlockwise, DeviceType);
        }
    }
}
