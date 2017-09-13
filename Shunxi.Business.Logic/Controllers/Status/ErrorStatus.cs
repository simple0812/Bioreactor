using System;
using Shunxi.Business.Models;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers.Status
{
    public class ErrorStatus : StatusBase
    {
        public ErrorStatus(ControllerBase controller) : base(controller)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, handle error");
        }

        public override void HandleIdle(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, can not receive Idle Directive");
        }

        public override void HandleTryStart(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, can not receive TryStart Directive");
        }

        public override void HandleRunning(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            Controller.ProcessRunningDirectiveResult(data, comEventArgs);
            LogFactory.Create()
                .Info(
                    $"device{Controller.Device.DeviceId} {OldStatus} status receive Running feedback convert to {comEventArgs.DeviceStatus}");
        }

        public override void HandleTryPause(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, can not receive TryPause Directive");
        }

        public override void HandlePausing(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, can not receive Pausing Directive");
        }

        public override void HandleClose(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            throw new NotImplementedException();
        }

        public override void HandleError(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            LogFactory.Create()
                .Warnning(
                    $"device{Controller.Device.DeviceId} SysStatus is {Controller.CurrentStatus}, handle error");
        }
    }
}
