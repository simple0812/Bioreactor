using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public class PausingDirective : BaseDirective
    {

        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.Pausing;
        public override int Priority => 4;

        public PausingDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
