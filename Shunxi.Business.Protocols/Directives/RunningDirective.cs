using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public class RunningDirective : BaseDirective
    {
        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.Running;
        public override int Priority => 5;

        public RunningDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
