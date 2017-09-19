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
    public class TemperatureController : ControllerBase
    {
        public TemperatureGauge TemperatureGauge;
        protected override int RunningPollingInterval => 10 * 1000;
        public override bool IsEnable => TemperatureGauge.IsEnabled;
        public TemperatureController(ControlCenter center, TemperatureDevice device, TemperatureGauge temperature):base(center, device)
        {
            TemperatureGauge = temperature;
        }

        public override async Task<DeviceIOResult> Start()
        {
            if(!TemperatureGauge.IsEnabled) return new DeviceIOResult(false, "DISABLED");

            StartTime = DateTime.Now;
            LogFactory.Create().Info($"start {Device.DeviceType}{Device.DeviceId} when SysStatus {CurrentStatus}");
            ((TemperatureDevice)Device).SetParams(TemperatureGauge.Temperature, TemperatureGauge.Level);
            SetStatus(DeviceStatusEnum.PreStart);
            Device.TryStart();

            StartEvent = new TaskCompletionSource<DeviceIOResult>();
            return await StartEvent.Task;
            //            var p = await StartEvent.WaitAsync(CancellationTokenSource.Token);
            //            StartEvent.Reset();
            //
            //            return p;
        }

        public override Task<DeviceIOResult> Close()
        {
            throw new NotImplementedException();
        }

        public override async Task<DeviceIOResult> Pause()
        {
            var p = await base.Pause();
            StartRunningLoop();
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

        //传感器只收发普通轮询指令
        public override void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            if (CurrentStatus == DeviceStatusEnum.Startting)
            {
                SetStatus(DeviceStatusEnum.Running);
                comEventArgs.DeviceStatus = DeviceStatusEnum.Running;

                StartEvent.TrySetResult(new DeviceIOResult(true));

                OnCommunicationChange(comEventArgs);
                StartRunningLoop();
            }
            else if (CurrentStatus == DeviceStatusEnum.Running)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
                StartRunningLoop();
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
            comEventArgs.Description = IdleDesc.Paused.ToString(); 
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

                var x = e.Data as TemperatureDirectiveData;
                if (x != null)
                {
                    DeviceService.SaveTemperatureRecord(new TemperatureRecord()
                    {
                        CellCultivationId = CultivationService.GetLastCultivationId(),
                        Temperature = x.CenterTemperature,
                        CreatedAt = DateTime.Now
                    });
                }

                Center.SyncTemperatureWithServer();
            }
        }

        private void HandleDeviceStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as TemperatureDirectiveData;
            if (data != null)
            {
                CurrentContext.SysCache.SystemRealTimeStatus.Temperature.Temperature = Math.Round(data.CenterTemperature, 2);
            }

            Center.OnDeviceStatusChange(e);
        }

    }
}
