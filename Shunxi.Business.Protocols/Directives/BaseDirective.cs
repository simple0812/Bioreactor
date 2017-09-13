using System.Threading;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Protocols.Directives
{
    public abstract class BaseDirective
    {
        public int DirectiveId { get; set; }
        public int TargetDeviceId { get; set; }
        private static int _directiveId = 65530;
        public abstract DirectiveTypeEnum DirectiveType { get; }
        public abstract int Priority { get; }
        public TargetDeviceTypeEnum DeviceType { get; set; }

        protected BaseDirective()
        {
            Interlocked.Increment(ref _directiveId);
            DirectiveId = _directiveId % 0xffff;
        }

        public static void ResetDirectiveId()
        {
            Interlocked.Exchange(ref _directiveId, 0);
        }

    }

}
