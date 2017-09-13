using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public class IdleDirective : BaseDirective
    {
        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.Idle;
        public override int Priority => 6;

        public IdleDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
