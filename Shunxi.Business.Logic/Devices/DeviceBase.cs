using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;

namespace Shunxi.Business.Logic.Devices
{
    public abstract class DeviceBase
    {
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int DeviceId { get; set; }

        protected abstract TryStartDirective GenerateTryStartDirective();

        public virtual void TryStart()
        {
            var directive = GenerateTryStartDirective();
            DirectiveWorker.Instance.PrepareDirective(directive);
        }

        public virtual void TryPause()
        {
            var directive = new TryPauseDirective(DeviceId, DeviceType);
            DirectiveWorker.Instance.PrepareDirective(directive);
        }

        public virtual void Stop()
        {
            var directive = new TryPauseDirective(DeviceId, DeviceType);
            DirectiveWorker.Instance.PrepareDirective(directive);
        }

        public virtual void Running()
        {
            var directive = new RunningDirective(DeviceId, DeviceType);
            DirectiveWorker.Instance.PrepareDirective(directive);
        }

        public virtual void Pausing()
        {
            var directive = new PausingDirective(DeviceId, DeviceType);
            DirectiveWorker.Instance.PrepareDirective(directive);
        }

        public virtual void Idle()
        {
            var directive = new IdleDirective(DeviceId, DeviceType);
            DirectiveWorker.Instance.PrepareDirective(directive);
        }
    }
}
