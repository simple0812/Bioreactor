using System;
using Shunxi.Business.Models;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers.Status
{
    public class PrePauseStatus : StatusBase
    {
        public PrePauseStatus(ControllerBase controller) : base(controller)
        {
        }

        public override void HandleIdle(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} CurrentStatus is {Controller.CurrentStatus}, can not receive Idle Directive");
        }

        public override void HandleTryStart(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} CurrentStatus is {Controller.CurrentStatus}, can not receive TryStart Directive");
        }

        public override void HandleRunning(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, can not receive Running Directive");
        }

        public override void HandleTryPause(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            Controller.ProcessTryPauseResult(data, comEventArgs);
            LogFactory.Create()
                .Info(
                    $"device{Controller.Device.DeviceId} {OldStatus} status receive TryPause feedback convert to {comEventArgs.DeviceStatus}");
        }

        public override void HandlePausing(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} CurrentStatus is {Controller.CurrentStatus}, can not receive Pausing Directive");
        }

        public override void HandleClose(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleError(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} CurrentStatus is {Controller.CurrentStatus}, handle error");
        }
    }
}
