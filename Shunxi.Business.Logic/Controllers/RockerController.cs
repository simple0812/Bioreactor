using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Devices;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.Business.Logic.Controllers
{
    public class RockerController : ControllerBase
    {
        public Rocker Rocker;
        protected override int RunningPollingInterval => 1000;
        public override bool IsEnable => Rocker.IsEnabled;

        public RockerController(ControlCenter center, RockerDevice device, Rocker rocker) : base(center, device)
        {
            Rocker = rocker;
        }

        public override async Task<DeviceIOResult> Start()
        {
            if (!IsEnable) return new DeviceIOResult(false, "DISABLED");

            StartTime = DateTime.Now;
            LogFactory.Create().Info($"start {Device.DeviceType}{Device.DeviceId} when SysStatus {CurrentStatus}");
            ((RockerDevice)Device).SetParams(Rocker.Speed, Rocker.Angle);
            SetStatus(DeviceStatusEnum.PreStart);
            Device.TryStart();
            StartEvent = new TaskCompletionSource<DeviceIOResult>();
            return await StartEvent.Task;
        }

        public override Task<DeviceIOResult> Close()
        {
            throw new NotImplementedException();
        }

        public override async Task<DeviceIOResult> Pause()
        {
            var p = await base.Pause();
            StartIdleLoop();
            return p;
        }

        public override void Next(SerialPortEventArgs args)
        {
            var comEventArgs = new CommunicationEventArgs
            {
                Command = args.Command,
                DirectiveId = args.Result.Data.DirectiveId,
                DeviceType = args.Result.Data.DeviceType,
                DeviceId = args.Result.Data.DeviceId,
                Description = args.Message,
                Data = args.Result.Data
            };

            this.Status.Handle(args.Result.SourceDirectiveType, args.Result.Data, comEventArgs);
            OnCommunication(comEventArgs);
        }

        public override void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            var ret = data as RockerDirectiveData;

            if (CurrentStatus == DeviceStatusEnum.Startting)
            {
                //从tryStart变为running(真的开始)
                if (ret?.Speed > 0 )
                {
                    CurrentStatus = DeviceStatusEnum.Running;
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
                    
                    StartEvent.TrySetResult(new DeviceIOResult(true));

                    OnCommunicationChange(comEventArgs);
                    StartRunningLoop();

                }
                else
                {
                    comEventArgs.DeviceStatus = CurrentStatus;
                    StartRunningLoop();
                }
            }
            else if (CurrentStatus == DeviceStatusEnum.Running)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
                StartRunningLoop();
            }
            else
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Error;
            }
        }

        public override void ProcessTryPauseResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            SetStatus(DeviceStatusEnum.Pausing);
            comEventArgs.DeviceStatus = DeviceStatusEnum.Pausing;
            OnCommunicationChange(comEventArgs);
            StartPauseLoop();
        }

        public override void ProcessPausingResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {

            comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
            comEventArgs.Description = IdleDesc.Paused.ToString(); ;
            SetStatus(DeviceStatusEnum.Idle);

            StopEvent.TrySetResult(new DeviceIOResult(true));

            OnCommunicationChange(comEventArgs);
        }

        public override void OnCommunication(CommunicationEventArgs e)
        {
            base.OnCommunication(e);

            if (e.Data.DirectiveType == DirectiveTypeEnum.Running 
                || e.Data.DirectiveType == DirectiveTypeEnum.Idle 
                || e.Data.DirectiveType == DirectiveTypeEnum.Pausing)
            {
                HandleDeviceStatusChange(new IoStatusChangeEventArgs()
                {
                    DeviceId = e.DeviceId,
                    DeviceType = e.DeviceType,
                    IoStatus = e.DeviceStatus,
                    Delta = 0,
                    Feedback = e.Data
                });

                Center.SyncRockerWithServer();
            }
        }

        private void HandleDeviceStatusChange(IoStatusChangeEventArgs e)
        {
            try
            {
                var feedback = e.Feedback;

                var data = feedback as RockerDirectiveData;
                if (data == null) return;


                CurrentContext.SysCache.SystemRealTimeStatus.Rocker.IsRunning = data.Speed > 0;
                CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Angle = data.Speed > 0 ? Config.DefaultAngle : 0;
                CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Speed = data.Speed;

                Center.OnDeviceStatusChange(e);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception.Message);
            }
           
        }
    }
}
