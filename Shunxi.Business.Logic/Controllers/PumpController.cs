using System;
using System.Linq;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Cultivations;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Tables;
using Shunxi.Infrastructure.Common.Extension;
using Shunxi.Business.Logic.Devices;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers
{
    public class PumpController : ControllerBase
    {
        public override bool IsEnable => true;

        //输入/输出的体积
        public double Volume;
        //输入/输出的速度
        public double Flowrate;
        //每次收到暂停反馈后需要设置上一次未完成的量 方便下次开始的时候设置参数
        public double PreUnfinishedVolume;
        public BaseCultivation PumpCultivation { get; set; }

        public PumpController(ControlCenter center, PumpDevice device, Pump pump) : base(center,device)
        {
            if (pump == null)
            {
                //CustomErrorEvent(new CustomException($"pump{PumpCultivation.PumpId} 的时间表排期为空", this.GetType().FullName, ExceptionPriority.Unrecoverable));
                return;
            }

            PumpCultivation = CultivationFactory.GetCultivation(pump);
            PumpCultivation.CalcSchedules();

            LogFactory.Create().Info($"pump{PumpCultivation.Device.DeviceId} {PumpCultivation.Device.ProcessMode} startTime is {PumpCultivation.Device.StartTime:yyyy-MM-dd HH:mm:ss}");
        }

        private double CalcPreUnfinishedVolume()
        {
            LogFactory.Create().Info($"{Volume},{Flowrate}<=============>{StartTime:yyyy-MM-dd HH:mm:ss},{StopTime:yyyy-MM-dd HH:mm:ss}");
            if (StartTime == DateTime.MinValue || StopTime == DateTime.MinValue || StartTime >= StopTime) return 0;
            var finished = (StopTime - StartTime).TotalMinutes * Flowrate;

            var ret = Volume - finished;
            //目前版本 流量参数不能为小数
            return ret >= 1 ? ret : 0;
        }

        private double CalcVolume(string desc)
        {
            var startTime = StartTime;
            var endTime = StopTime;

            if (desc == IdleDesc.Completed.ToString())
            {
                return Volume;
            }

            if (desc == IdleDesc.Paused.ToString() && startTime < endTime)
            {
                return Flowrate * (endTime - startTime).TotalMinutes;
            }

            return 0;
        }

        private Tuple<double, double, double> PreStart()
        {
            var interval = 0;
            var flowrate = 0D;
            var volume = 0D;
            //TODO 如果上次未完成 则立即开始运行
            if (PreUnfinishedVolume > 0)
            {
                flowrate = Flowrate;
                volume = PreUnfinishedVolume;
                LogFactory.Create().Info($"pump{PumpCultivation.Device.DeviceId} pre unfinished {PreUnfinishedVolume:##.###}, starttime{DateTime.Now:yyyy-MM-dd HH:mm:ss}");
                //清理数据 防止影响下一个流程
                PreUnfinishedVolume = 0;
            }
            else
            {
                var currentDateTime = DateTime.Now;
                var next = PumpCultivation.GetNextRunParams(AlreadyRunTimes == 0);
                //暂停的时候泵还在运行， 但重新开始后由于需要加入的量很少（<1）所以认为上次已经加完，此时需要重新判断泵的流程是否全部完成
                if (null == next)
                {
                    LogFactory.Create().Info($"DEVICE{Device.DeviceId} cultivate process finished");
                    if (CurrentStatus != DeviceStatusEnum.AllFinished)
                    {
                        var comEventArgs = new CommunicationEventArgs
                        {
                            DeviceType = TargetDeviceTypeEnum.Pump,
                            DeviceId = PumpCultivation.Device.DeviceId,
                            DeviceStatus = DeviceStatusEnum.Idle,
                            Description = IdleDesc.Completed.ToString()
                        };

                        OnCommunicationChange(comEventArgs);
                    }

                    //整个流程已完成
                    return null;
                }

                var nextTime = next.Item1;

                var timeSpan = nextTime > currentDateTime ? (DateTime)nextTime - currentDateTime : TimeSpan.FromSeconds(0);
                interval = (int)timeSpan.TotalMilliseconds;
                flowrate = next.Item2;
                volume = next.Item3;
                LogFactory.Create().Info($"pump{Device.DeviceId} prev is finished, next starttime{nextTime:yyyy-MM-dd HH:mm:ss}, interval{timeSpan.TotalMilliseconds}");
            }

            if (interval < 0) interval = 0;

            return new Tuple<double, double, double>(interval, flowrate, volume);
        }

        public override async Task<DeviceIOResult> Start()
        {
            var pre = PreStart();

            if (pre == null)
            {
                LogFactory.Create().Info($"DEVICE{Device.DeviceId} completed finished");
                return new DeviceIOResult(false, "DISABLED");
            }

            var interval = pre.Item1;
            var flowrate = pre.Item2;
            var volume = pre.Item3;

            try
            {
                await Task.Delay((int)interval, CancellationTokenSource.Token);
                return await ExecStart(flowrate, volume);
            }
            catch (Exception)
            {
                LogFactory.Create().Info($"DEVICE{Device.DeviceId} start is cancel");
            }

            return new DeviceIOResult(false,"CANCEL");
        }

        private async Task<DeviceIOResult> ExecStart(double flowrate, double volume)
        {
            //出液泵开启前摇床要停止，出液泵停止后泵摇床开始
            if (PumpCultivation.Device.InOrOut == PumpInOrOut.Out && CurrentContext.SysCache.System.Rocker.IsEnabled)
            {
                await Center.StopRockerAndThermometer();
            }

            Flowrate = flowrate;
            Volume = volume;
            ((PumpDevice)Device).SetParams(Flowrate, Volume, PumpCultivation.Device.Direction);

            StartTime = DateTime.Now;
            SetStatus(DeviceStatusEnum.PreStart);
            Device.TryStart();
            StartEvent = new TaskCompletionSource<DeviceIOResult>();
            return await StartEvent.Task;
//            var p = await StartEvent.WaitAsync(CancellationTokenSource.Token);
//            StartEvent.Reset();
//            return p;
        }

        public override Task<DeviceIOResult> Close()
        {
            throw new NotImplementedException();
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

        public void AdjustTimeForPause()
        {
            if(CurrentStatus == DeviceStatusEnum.AllFinished) return;

            PumpCultivation.AdjustTimeForPause(StopTime == DateTime.MinValue ? Center.StopTime : StopTime);
        }

        public void AdjustStartTimeWhenFirstRun(TimeSpan span)
        {
            LogFactory.Create().Info($"{Device.DeviceId} need adjust time");
            PumpCultivation.AdjustStartTimeWhenFirstRun(span);
        }

        public override void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            var ret = data as PumpDirectiveData;

            if (CurrentStatus == DeviceStatusEnum.Startting)
            {
                //从tryStart变为running(真的开始)
                if (ret?.FlowRate > 0)
                {
                    this.SetStatus(DeviceStatusEnum.Running);
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Running;

                    StartEvent.TrySetResult(new DeviceIOResult(true));

                    OnCommunicationChange(comEventArgs);
                }
                else//泵收到开始命令 但还未运行
                {
                    comEventArgs.DeviceStatus = CurrentStatus;
                }

                StartRunningLoop();
            }
            else if (CurrentStatus == DeviceStatusEnum.Running)
            {
                //泵输入/输出指定流量后停止
                if (ret != null && ret.FlowRate <= 0)
                {
                    this.SetStatus(DeviceStatusEnum.Idle);
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
                    comEventArgs.Description = IdleDesc.Completed.ToString();
                    OnCommunicationChange(comEventArgs);
                }
                else//泵正在运行
                {
                    comEventArgs.DeviceStatus = DeviceStatusEnum.Running;
                    StartRunningLoop();
                }
            }
        }

        public override void ProcessTryPauseResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            this.SetStatus(DeviceStatusEnum.Pausing);
            comEventArgs.DeviceStatus = DeviceStatusEnum.Pausing;
            OnCommunicationChange(comEventArgs);
            StartPauseLoop();
        }

        public override void ProcessPausingResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            var ret = data as PumpDirectiveData;

            if (ret != null && ret.FlowRate <= 0)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
                comEventArgs.Description = IdleDesc.Paused.ToString(); ;

                this.SetStatus(DeviceStatusEnum.Idle);

                StopEvent.TrySetResult(new DeviceIOResult(true));
                StopTime = DateTime.Now;
                PreUnfinishedVolume = CalcPreUnfinishedVolume();
                LogFactory.Create().Info($"device{Device.DeviceId} PreUnfinishedVolume is {PreUnfinishedVolume}");
                OnCommunicationChange(comEventArgs);
            }
            else
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Pausing;
                StartPauseLoop();
            }
        }

        public override void OnCommunicationChange(CommunicationEventArgs e)
        {
            base.OnCommunicationChange(e);
            //HandleSystemStatusChange(e);

            HandleDeviceStatusChange(e);


            if (e.DeviceStatus != DeviceStatusEnum.Startting && e.DeviceStatus != DeviceStatusEnum.Pausing)
            {
                var data = e.Data as PumpDirectiveData;
                if (data != null)
                    Center.SyncPumpWithServer(data.DeviceId);
            }
        }

        protected void HandleDeviceStatusChange(CommunicationEventArgs e)
        {
            var deviceId = e.DeviceId;
            var type = e.DeviceStatus;
            var feedback = e.Data;
            var delta = CalcVolume(e.Description);
            var ftime = PumpCultivation.Schedules.FirstOrDefault();
            var ltime = PumpCultivation.Schedules.LastOrDefault();
            var stime = StartTime;
            var etime = Flowrate <=0 ? DateTime.MinValue : StartTime.AddMinutes((PreUnfinishedVolume > 0 ? PreUnfinishedVolume : Volume) / Flowrate);
            var ntime = PumpCultivation.Schedules.FirstOrDefault(each => each > DateTime.Now);

            switch (type)
            {
                case DeviceStatusEnum.Startting:
                    CurrentContext.SysCache.SystemRealTimeStatus.Update(deviceId, true, Volume, 0,
                        this.AlreadyRunTimes, PumpCultivation.Schedules.Count, ftime, ltime, stime, etime, ntime);
                    break;
                case DeviceStatusEnum.Running:
                    var data = feedback as PumpDirectiveData;
                    if (data != null)
                    {
                        CurrentContext.SysCache.SystemRealTimeStatus.Update(deviceId, true, Volume, data.FlowRate, AlreadyRunTimes);
                    }
                    break;
                //case DeviceStatusEnum.Pausing:
                case DeviceStatusEnum.Idle:
                    if (CurrentContext.SysCache.SystemRealTimeStatus.In.DeviceId == deviceId)
                    {
                        CurrentContext.SysCache.SystemRealTimeStatus.CurrVolume += Convert.ToInt32(delta);
                    }
                    else
                    {
                        CurrentContext.SysCache.SystemRealTimeStatus.CurrVolume -= Convert.ToInt32(delta);
                    }
                    HandleSystemStatusChange(e);
                    CurrentContext.SysCache.SystemRealTimeStatus.Update(deviceId, false, Volume, 0, AlreadyRunTimes);
                    break;
                default:
                    break;
            }

            //涉及到动画, starting Running都会启动动画 只需要触发一次
            if (type == DeviceStatusEnum.Running || type == DeviceStatusEnum.Idle)
                Center.OnDeviceStatusChange(new IoStatusChangeEventArgs()
                {
                    DeviceId = e.DeviceId,
                    DeviceType = e.DeviceType,
                    IoStatus = e.DeviceStatus,
                    Delta = delta,
                    Feedback = e.Data
                });
        }

        private void SaveRecord(bool isManual)
        {
            DeviceService.SavePumpRecord(new PumpRecord()
            {
                CellCultivationId = CultivationService.GetLastCultivationId(),
                DeviceId = Device.DeviceId,
                StartTime = StartTime,
                EndTime = DateTime.Now,
                FlowRate = Flowrate,
                Volume = Volume,
                IsManual = isManual
            });
        }

        private void HandleSystemStatusChange(CommunicationEventArgs e)
        {
            var p = new RunStatusChangeEventArgs();
            //系统运行状态只与泵关联
            if (e.DeviceType != TargetDeviceTypeEnum.Pump || e.DeviceStatus != DeviceStatusEnum.Idle) return;

            if (e.Description == IdleDesc.Paused.ToString())//设备暂停运行
            {
                SaveRecord(true);

                if (CurrentContext.Status == SysStatusEnum.Pausing)
                {
                    p.SysStatus = SysStatusEnum.Paused;
                    Center.OnSystemStatusChange(p);
                }
                else if (CurrentContext.Status == SysStatusEnum.Discarding)
                {
                    p.SysStatus = SysStatusEnum.Discarded;
                    Center.OnSystemStatusChange(p);
                }

            }
            else if (e.Description == IdleDesc.Completed.ToString())//设备完成本次操作
            {
                SaveRecord(false);

                AlreadyRunTimes++;
                var next = PumpCultivation.GetNextRunParams(AlreadyRunTimes == 0);
                //该泵整个培养流程完成
                if (next == null || PumpCultivation.Device.ProcessMode == ProcessModeEnum.SingleMode)
                {
                    LogFactory.Create().Info($"{e.DeviceId} cultivation finished");
                    SetStatus(DeviceStatusEnum.AllFinished);


                    //如果所有泵都已经完成 则通知前端培养流程完成
                    if (Center.IsAllFinished())
                    {
                        LogFactory.Create().Info("all cultivation finished");

                        p.SysStatus = SysStatusEnum.Completed;
                        Center.OnSystemStatusChange(p);
                    }
                }
                //该泵完成本次进液或出液流程
                else
                {
                    LogFactory.Create().Info($"<->pump{e.DeviceId} {AlreadyRunTimes}times completed");
                    StartNextLoop(next).IgnorCompletion();
                    StartIdleLoop();
                }
                //培养流程结束后 然后要保证摇床和温度打开
                if (PumpCultivation.Device.InOrOut == PumpInOrOut.Out)
                {
                    Center.StartRockerWhenPumpOutStop().IgnorCompletion();
                    //                            Center.StartRockerAndThermometer().IgnorCompletion();
                }

                Center.StartThermometerWhenPumpStop(CurrentContext.SysCache.SystemRealTimeStatus.CurrVolume).IgnorCompletion();
            }
        }

        private async Task StartNextLoop(Tuple<DateTime, double, double> next)
        {
            if (next != null)
            {
                var interval = (int)((DateTime)next.Item1 - DateTime.Now).TotalMilliseconds;
                LogFactory.Create().Info($"StartNextLoop pump{PumpCultivation.Device.DeviceId} starttime{next.Item1:yyyy-MM-dd HH:mm:ss}, interval{interval}");
                if (interval < 0)
                    interval = 0;
                try
                {
                    await Task.Delay(interval, CancellationTokenSource.Token);
                    await ExecStart(next.Item2, next.Item3);
                }
                catch (Exception)
                {
                    LogFactory.Create().Info($"DEVICE{Device.DeviceId} StartNextLoop is cancel");
                }
            }
        }
    }
}
