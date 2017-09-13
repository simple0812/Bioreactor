using System;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers.Status
{
    public class IdleStatus : StatusBase
    {
        public IdleStatus(ControllerBase controller) : base(controller)
        {
        }

        public override void HandleIdle(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            Controller.SetStatus(DeviceStatusEnum.Idle);
            comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;

            Controller.StartIdleLoop();
            LogFactory.Create()
                .Info(
                    $"device{Controller.Device.DeviceId} {OldStatus} status receive Idle feedback convert to {comEventArgs.DeviceStatus}");
        }

        public override void HandleTryStart(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            Controller.ProcessTryStartResult(data, comEventArgs);
            LogFactory.Create()
                .Info(
                    $"device{Controller.Device.DeviceId} {OldStatus} status receive TryStart feedback convert to {comEventArgs.DeviceStatus}");
        }

        //温度使用04命令代替03命令轮询
        public override void HandleRunning(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            Controller.ProcessRunningDirectiveResult(data, comEventArgs);
            LogFactory.Create()
                .Info(
                    $"device{Controller.Device.DeviceId} {OldStatus} status receive Running feedback convert to {comEventArgs.DeviceStatus}");
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
