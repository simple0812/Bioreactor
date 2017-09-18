using System;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Devices;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Tables;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers
{
    public class GasController: ControllerBase
    {
        public Gas Gas;
        protected override int RunningPollingInterval => 10 * 1000;
        public override bool IsEnable => Gas.IsEnabled;
        public GasController(ControlCenter center, GasDevice device, Gas gas):base(center, device)
        {
            Gas = gas;
        }

        public override async Task<DeviceIOResult> Start()
        {
            if(!IsEnable) return new DeviceIOResult(false, "DISABLED");

            StartTime = DateTime.Now;
            LogFactory.Create().Info($"start {Device.DeviceType}{Device.DeviceId} when SysStatus {CurrentStatus}");
            ((GasDevice)Device).SetParams(Gas.FlowRate, Gas.Concentration);
            SetStatus(DeviceStatusEnum.PreStart);
            Device.TryStart();
            StartEvent = new TaskCompletionSource<DeviceIOResult>();
            return await StartEvent.Task;
        }

        public override Task<DeviceIOResult> Close()
        {
            throw new NotImplementedException();
        }

        public override Task<DeviceIOResult> Pause()
        {
            var p = base.Pause();
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
            if (CurrentStatus == DeviceStatusEnum.Startting)
            {
                CurrentStatus = DeviceStatusEnum.Running;
                comEventArgs.DeviceStatus = DeviceStatusEnum.Running;

                StartEvent.TrySetResult(new DeviceIOResult(true));

                OnCommunicationChange(comEventArgs);
                StartRunningLoop();
            }
            else if (CurrentStatus == DeviceStatusEnum.Running)
            {
                //TODO 摇床速度不应该自动变为0
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
            comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
            comEventArgs.Description = IdleDesc.Paused.ToString(); ;
            SetStatus(DeviceStatusEnum.Running);

            StopEvent.TrySetResult(new DeviceIOResult(true));

            OnCommunicationChange(comEventArgs);
        }


        public override void OnCommunication(CommunicationEventArgs e)
        {
            base.OnCommunication(e);

            if (e.Data.DirectiveType == DirectiveTypeEnum.Running)
            {
                HandleDeviceStatusChange(new IoStatusChangeEventArgs()
                {
                    DeviceId = e.DeviceId,
                    DeviceType = e.DeviceType,
                    IoStatus = e.DeviceStatus,
                    Delta = 0,
                    Feedback = e.Data
                });
                var x = e.Data as GasDirectiveData;
                if (x != null)
                {
                    DeviceService.SaveGasRecord(new GasRecord()
                    {
                        CellCultivationId = CultivationService.GetLastCultivationId(),
                        Concentration = x.Concentration,
                        FlowRate = x.Flowrate,
                        CreatedAt = DateTime.Now
                    });
                }

                Center.SyncGasWithServer();
            }
        }

        private void HandleDeviceStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as GasDirectiveData;
            if (data != null)
            {
                CurrentContext.SysCache.SystemRealTimeStatus.Gas.FlowRate = data.Flowrate;
                CurrentContext.SysCache.SystemRealTimeStatus.Gas.Concentration = data.TimeInterval;
            }

            Center.OnDeviceStatusChange(e);
        }
    }
}
