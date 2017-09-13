using System;
using System.Collections.Generic;
using Shunxi.Business.Enums;
using Shunxi.Business.Models;

namespace Shunxi.Business.Logic.Controllers.Status
{
    public abstract class StatusBase
    {
        public ControllerBase Controller;
        protected DeviceStatusEnum OldStatus;

        protected StatusBase(ControllerBase controller)
        {
            Controller = controller;
        }

        public void Handle(DirectiveTypeEnum type, DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            OldStatus = Controller.CurrentStatus;

            switch (type)
            {
                case DirectiveTypeEnum.Idle:
                    HandleIdle(data, comEventArgs);break;
                case DirectiveTypeEnum.TryStart:
                    HandleTryStart(data, comEventArgs); break;
                case DirectiveTypeEnum.Running:
                    HandleRunning(data, comEventArgs); break;
                case DirectiveTypeEnum.TryPause:
                    HandleTryPause(data, comEventArgs); break;
                case DirectiveTypeEnum.Pausing:
                    HandlePausing(data, comEventArgs); break;
                case DirectiveTypeEnum.Close:
                    HandleClose(data, comEventArgs); break;
                default:
                    HandleError(data, comEventArgs); break;
            }
        }
        //收到Idle反馈后的处理方法
        public abstract void HandleIdle(DirectiveData data, CommunicationEventArgs comEventArgs);
        //收到TryStart反馈后的处理方法
        public abstract void HandleTryStart(DirectiveData data, CommunicationEventArgs comEventArgs);
        //收到Running反馈后的处理方法
        public abstract void HandleRunning(DirectiveData data, CommunicationEventArgs comEventArgs);
        //收到TryPause反馈后的处理方法
        public abstract void HandleTryPause(DirectiveData data, CommunicationEventArgs comEventArgs);
        //收到Pasuing反馈后的处理方法
        public abstract void HandlePausing(DirectiveData data, CommunicationEventArgs comEventArgs);
        //收到Close反馈后的处理方法
        public abstract void HandleClose(DirectiveData data, CommunicationEventArgs comEventArgs);
        public abstract void HandleError(DirectiveData data, CommunicationEventArgs comEventArgs);
    }
}
