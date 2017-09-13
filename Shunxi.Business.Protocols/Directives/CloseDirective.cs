using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public class CloseDirective : BaseDirective
    {
        public override DirectiveTypeEnum DirectiveType => DirectiveTypeEnum.Close;
        public override int Priority => 1;

        public CloseDirective(int targetDeviceId, TargetDeviceTypeEnum deviceType = TargetDeviceTypeEnum.Pump)
        {
            this.TargetDeviceId = targetDeviceId;
            this.DeviceType = deviceType;
        }
    }
}
