using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic.Devices
{
    public class PumpDevice: DeviceBase
    {
        public double FlowRate { get; set; } = Config.DefaultFlowRate;
        public double Volume { get; set; } = 20;
        public DirectionEnum Direction { get; set; }

        public PumpDevice(int deviceId = 0x01, DirectionEnum direction = DirectionEnum.In)
        {
            DeviceType = TargetDeviceTypeEnum.Pump;
            DeviceId = deviceId;
            Direction = direction;
        }

        public PumpDevice SetParams(double flowRate, double volume)
        {
            FlowRate = flowRate;
            Volume = volume;

            return this;
        }

        protected override TryStartDirective GenerateTryStartDirective()
        {
            return new TryStartDirective(DeviceId, FlowRate, Volume, (int)Direction);
        }

    }
}
