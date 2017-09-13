using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic.Devices
{
    public class RockerDevice: DeviceBase
    {
        public double Speed { get; set; } = Config.RockerSpeed;

        //暂时代替为温度
        public double Angle { get; set; } = 12;
        public RockEnum RockMode { get; set; } = RockEnum.Normal;

        public RockerDevice(int deviceId = 0x80)
        {
            DeviceId = deviceId;
            DeviceType = TargetDeviceTypeEnum.Rocker;
        }

        public void SetParams(double speed, double angle)
        {
            Speed = speed;
            Angle = angle;
        }

        protected override TryStartDirective GenerateTryStartDirective()
        {
            return new TryStartDirective(DeviceId, Speed, Angle, (int)RockMode, DeviceType);
        }

    }
}
