using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;

namespace Shunxi.Business.Logic.Devices
{
    public class TemperatureDevice: DeviceBase
    {

        public double Temperature { get; set; }
        public CapacityLevel Level { get; set; }

        public TemperatureDevice(int deviceId = 0x90)
        {
            DeviceId = deviceId;
            DeviceType = TargetDeviceTypeEnum.Temperature;
        }

        public void SetParams(double temp, CapacityLevel level)
        {
            Temperature = temp;
            Level = level;
        }

        protected override TryStartDirective GenerateTryStartDirective()
        {
            return new TryStartDirective(DeviceId, Temperature * 10, Temperature * 10, (int)Level,
                TargetDeviceTypeEnum.Temperature);
        }
    }
}
