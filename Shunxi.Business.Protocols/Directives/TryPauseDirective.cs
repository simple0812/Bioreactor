using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public class TryPauseDirective : BaseDirective
    {
        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.TryPause;
        public override int Priority => 2;

        public TryPauseDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
