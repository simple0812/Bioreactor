 using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
 using Shunxi.Business.Logic.Devices;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;
 using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers
{
    public class ControlCenter
    {
        public IList<ControllerBase> Controllers = new List<ControllerBase>();
        //如果设备还未开始 暂停的时候设备无法记录暂停时间 但是设备重新开始的时候需要使用暂停时长来重新调整排期
        //此时可以使用该时间
        public DateTime StopTime { get; set; }

        public PumpController PumpCtrl;

        private ControlCenter()
        {
            AttachEvent();
        }

        #region 懒汉单例模式
        private static volatile ControlCenter _instance = null;
        private static readonly object InstanceLocker = new object();
        public static ControlCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (InstanceLocker)
                    {
                        if (_instance == null)
                            _instance = new ControlCenter();
                    }
                }
                return _instance;
            }
        }
        #endregion

        public void AttachEvent()
        {
            try
            {
                DirectiveWorker.Instance.SerialPortEvent -= Instance_SerialPortEvent;
                DirectiveWorker.Instance.ErrorEvent -= Instance_ErrorEvent;
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("AttachEvent" + e.Message);
            }

            DirectiveWorker.Instance.SerialPortEvent += Instance_SerialPortEvent;
            DirectiveWorker.Instance.ErrorEvent += Instance_ErrorEvent;
        }

        public bool IsAllFinished()
        {
          return Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Pump).All(each => each.CurrentStatus == DeviceStatusEnum.AllFinished);
        }

        public async Task ReStart()
        { 
            if (CurrentContext.SysCache.System == null)
            {
                LogFactory.Create().Warnning("SysCache.System is null");
                return;
            }

            if (CurrentContext.SysCache.System.PumpIn == null && CurrentContext.SysCache.System.PumpOut == null)
            {
                LogFactory.Create().Warnning("SysCache.System in or out is null");
                return;
            }

            var pumpCtrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Pump).Select(each => each as PumpController).ToList();

            //如果是暂停后重新开始 需要修正暂停的时间
            pumpCtrls.ForEach(each =>
            {
                each.AdjustTimeForPause();
            });

            OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Starting });
            SyncSysStatusWithServer();

            var nonPumpCtrls =
                Controllers.Where(each => each.Device.DeviceType != TargetDeviceTypeEnum.Pump).ToList();

            foreach (var t in nonPumpCtrls)
            {
                var p = await t.ReStart();
                if (!p.Status && p.Code == "CANCEL") return;
            }
            
            var list = new List<Task<DeviceIOResult>>();
            pumpCtrls.ForEach(each =>
            {
                list.Add(each.ReStart());
            });

            await Task.WhenAny(list);
        }

        private void InitControllers()
        {
            Controllers = new List<ControllerBase>();

            Controllers.Add(new TemperatureController(this, new TemperatureDevice(),
                CurrentContext.SysCache.System.TemperatureGauge));

            Controllers.Add(new RockerController(this, new RockerDevice(),
                CurrentContext.SysCache.System.Rocker));

            Controllers.Add(new GasController(this, new GasDevice(),
                CurrentContext.SysCache.System.Gas));

            Controllers.Add(new PumpController(this, new PumpDevice { DeviceId = 0x01 },
                CurrentContext.SysCache.System.PumpIn));

            Controllers.Add(new PumpController(this, new PumpDevice { DeviceId = 0x03 },
                CurrentContext.SysCache.System.PumpOut));
        }

        public async Task Start()
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            InitControllers();
            sw.Stop();
            LogFactory.Create().Info($"init controllers {sw.ElapsedMilliseconds} ms");
            if (CurrentContext.SysCache.System == null)
            {
                LogFactory.Create().Warnning("SysCache.System is null");
                return;
            }

            if (CurrentContext.SysCache.System.PumpIn == null && CurrentContext.SysCache.System.PumpOut == null)
            {
                LogFactory.Create().Warnning("SysCache.System in or out is null");
                return;
            }

            OnSystemStatusChange(new RunStatusChangeEventArgs() {SysStatus = SysStatusEnum.Starting});
            CultivationService.SaveCultivations(CurrentContext.SysCache.System);
            SyncSysStatusWithServer();


            var pumpCtrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Pump).Select(each => each as PumpController).ToList();

            var nonPumpCtrls =
                Controllers.Where(each => each.Device.DeviceType != TargetDeviceTypeEnum.Pump).ToList();

            foreach (var ctrl in nonPumpCtrls)
            {
                var p = await ctrl.Start();
                if (!p.Status && p.Code == "CANCEL") return;
            }

            AdjuestStartTimeWhenFirstRun();

            var list = new List<Task<DeviceIOResult>>();

            pumpCtrls.ForEach(each =>
            {
                list.Add(each.Start());
            });

            await Task.WhenAny(list);
        }

        private void AdjuestStartTimeWhenFirstRun()
        {
            var now = DateTime.Now;
            var pumpCtrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Pump).Select(each => each as PumpController).ToList();
            if(pumpCtrls.Count == 0) return;
            var p = pumpCtrls.OrderBy(each => each.PumpCultivation.Device.StartTime).FirstOrDefault();
            if(p == null) return;

            var firstTime = p.PumpCultivation.Device.StartTime;
            if (firstTime > now) return;

            var pastTime = now - firstTime;
            LogFactory.Create().Info($"starttime have passed {pastTime.TotalMinutes} minutes, need adjust");

            pumpCtrls.ForEach(each =>
            {
                each.AdjustStartTimeWhenFirstRun(pastTime);
            });
        }

        public async Task Close()
        {
            //1.取消监听事件
            DetachEvent();
            //2.取消设备待发送的指令
            DevicesCancel();
            //3.清理待发送队列中的指令
            //4.清理待反馈队列中的指令
            DirectiveWorker.Instance.Clean();
            //5.重新监听事件
            AttachEvent();
            var task = StopDevices();
            var cancellationToken = new CancellationTokenSource(5 * 1000).Token;
            var cancellationCompletionSource = new TaskCompletionSource<bool>();

            using (cancellationToken.Register(() => cancellationCompletionSource.TrySetResult(true)))
            {
                if (task != await Task.WhenAny(task, cancellationCompletionSource.Task))
                {
                    Dispose();
                    return;
                }
            }

            await task;
            Dispose();
        }

        private async Task StopDevices()
        {
            OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Discarding });
            var nonPumpCtrls = Controllers.Where(each => each.Device.DeviceType != TargetDeviceTypeEnum.Pump && each.Device.DeviceType != TargetDeviceTypeEnum.Rocker);
            foreach (var each in nonPumpCtrls)
            {
                await each.Stop();
            }

            var ret = await StopPumps();

            //如果所有泵为空闲状态、未开始状态、已完成状态则不会向泵发送指令 导致需要手动修改系统状态
            if (ret.All(p => p.Status == false))
            {
                OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Discarded });
                SyncDeviceDataWithServer();
            }
            var ctrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Rocker).ToList();

            foreach (var each in ctrls)
            {
                await each.Pause();
            }
        }

        public void Dispose()
        {
            try
            {
                foreach (var item in Controllers)
                {
                    item.Dispose();
                }

                Controllers.Clear();
                DirectiveWorker.Instance.Clean();
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("control center dispose error->" + e.Message);
            }
        }

        private void DevicesCancel()
        {
            foreach (var item in Controllers)
            {
                item.Cancel();
            }
        }
        //TODO 取消
        public async Task Pause()
        {
            //1.取消监听事件
            DetachEvent();
            //2.取消设备待发送的指令
            DevicesCancel();
            //3.清理待发送队列中的指令
            //4.清理待反馈队列中的指令
            DirectiveWorker.Instance.Clean();
            //5.重新监听事件
            AttachEvent();
            OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Pausing });
            StopTime = DateTime.Now;
            var nonPumpCtrls = Controllers.Where(each => each.Device.DeviceType != TargetDeviceTypeEnum.Pump);
            foreach (var each in nonPumpCtrls)
            {
                 await each.Pause();
            }
            var ret = await StopPumps();
            if (ret.All(p => p.Status == false))
            {
                OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Paused });
                SyncDeviceDataWithServer();
            }
        }

        public virtual void DetachEvent()
        {
            DirectiveWorker.Instance.SerialPortEvent -= Instance_SerialPortEvent;
            DirectiveWorker.Instance.ErrorEvent -= Instance_ErrorEvent;
        }

        #region 设备控制
       

        //摇床开启的时候关闭加热器 防止温度分部不均
        //该方法在暂停或者出液泵开启的时候使用
        public async Task StopRockerAndThermometer()
        {
            var ctrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Rocker || each.Device.DeviceType == TargetDeviceTypeEnum.Temperature).ToList();

            foreach (var each in ctrls)
            {
                await each.Pause();
            }
        }

        public async Task StopGases()
        {
            var ctrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Gas);
            foreach (var each in ctrls)
            {
                await each.Pause();
            }
        }

        public async Task<DeviceIOResult[]> StopPumps()
        {
            var ctrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Pump);
            var list = new List<Task<DeviceIOResult>>();

            foreach (var each in ctrls)
            {
                list.Add(each.Stop());
            }

            return await Task.WhenAll(list);
        }

        public async Task<DeviceIOResult> StartRockerAndThermometer()
        {
            var ctrls = Controllers.Where(each => each.Device.DeviceType == TargetDeviceTypeEnum.Temperature || each.Device.DeviceType == TargetDeviceTypeEnum.Rocker).ToList();
            if (!ctrls.Any())
                return new DeviceIOResult(false, "UNINITIALIZED");

            foreach (var each in ctrls)
            {
                var ret = await each.Start();
                if (!ret.Status && ret.Code == "CANCEL") return ret;
            }

            return new DeviceIOResult(true);
        }

        public Task StartSinglePump(int pumpId, DirectionEnum direction, double flowRate, double volume)
        {
            return null;
//            if (PumpCtrl == null)
//            {
//                PumpCtrl = new PumpController(this, new PumpDevice(pumpId, direction), new SingleCultivation() {Direction = direction});
//            }
//
//            await PumpCtrl.Device.SetParams(flowRate, volume).StartAsync();
        }

        public Task StopSinglePump()
        {
            return null;
            //            if (singleDevice != null)
            //            {
            //                await singleDevice.StopAsync();
            //                singleDevice.Dispose();
            //                singleDevice = null;
            //            }
        }

        #endregion
        private void Instance_ErrorEvent(CustomException arg1, BaseDirective arg2)
        {
            OnErrorEvent(arg1);
        }

        private void Instance_SerialPortEvent(SerialPortEventArgs args)
        {
            if (args.Result?.Data == null || args.Result.Data.DeviceId <= 0)
            {
                LogFactory.Create().Warnning("receive data is illness");
                return;
            }

            if (!args.IsSucceed)
            {
                // 错误处理
                LogFactory.Create().Warnning($"device{args.Result.Data.DeviceId} receive Directive failed");
                return;
            }

            var ctrl = GetControllerById(args.Result.Data.DeviceId);
            ctrl?.Next(args);
        }

        public ControllerBase GetControllerById(int id)
        {
            return Controllers?.FirstOrDefault(each => each.Device.DeviceId == id);
        }

        #region 事件
        public delegate void RunStatusChangeEventHandler(object sender, RunStatusChangeEventArgs e);
        public delegate void IoStatusChangeEventHandler(object sender, IoStatusChangeEventArgs e);
        public delegate void ErrorEventHandler(object sender, CustomException e);

        public event RunStatusChangeEventHandler SystemStatusChangeEvent;
        public event IoStatusChangeEventHandler DeviceStatusChangeEvent;
        public event ErrorEventHandler ErrorEvent;

        public void OnSystemStatusChange(RunStatusChangeEventArgs args)
        {
            CurrentContext.Status = args.SysStatus;

            if (CurrentContext.SysCache != null)
            {
                CurrentContext.SysCache.SystemRealTimeStatus.CurrStatus = args.SysStatus;
            }

            SystemStatusChangeEvent?.Invoke(this, args);
        }

        public void OnDeviceStatusChange(IoStatusChangeEventArgs args)
        {
            DeviceStatusChangeEvent?.Invoke(this, args);
        }

        public void OnErrorEvent(CustomException e)
        {
            ErrorEvent?.Invoke(this, e);
        }

        #endregion

        #region 远程控制同步
        public void SyncDeviceDataWithServer()
        {
            try
            {
                WsClient.Instance.Send(CurrentContext.SysCache?.SyncCurTime(), "syncData");
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("SyncDeviceDataWithServer->" + e.Message);
            }
        }

        public void SyncPumpWithServer(int deviceId)
        {
            if (CurrentContext.SysCache?.SystemRealTimeStatus == null) return;

            try
            {
                if (deviceId == 1)
                {
                    WsClient.Instance.Send(new
                    {
                        CurrVolume = CurrentContext.SysCache?.SystemRealTimeStatus.CurrVolume,
                        In = CurrentContext.SysCache?.SystemRealTimeStatus.In,
                        CurrStatus = CurrentContext.SysCache?.SystemRealTimeStatus.CurrStatus.ToString()
                    }, "syncPump");

                }
                else
                {
                    WsClient.Instance.Send(new
                    {
                        CurrVolume = CurrentContext.SysCache.SystemRealTimeStatus.CurrVolume,
                        Out = CurrentContext.SysCache?.SystemRealTimeStatus.Out,
                        CurrStatus = CurrentContext.SysCache?.SystemRealTimeStatus.CurrStatus.ToString()
                    }, "syncPump");

                }
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("SyncDeviceDataWithServer->" + e.Message);
            }
        }

        public void SyncTemperatureWithServer()
        {
            try
            {
                WsClient.Instance.Send(new { Temperature = CurrentContext.SysCache.SystemRealTimeStatus.Temperature.Temperature, CurrStatus = CurrentContext.SysCache?.SystemRealTimeStatus.CurrStatus.ToString() }, "syncTemperature");
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("SyncTemperatureWithServer->" + e.Message);
            }
        }

        public void SyncRockerWithServer()
        {
            try
            {
                WsClient.Instance.Send(new { Rocker = CurrentContext.SysCache.SystemRealTimeStatus.Rocker, CurrStatus = CurrentContext.SysCache?.SystemRealTimeStatus.CurrStatus.ToString() }, "syncRocker");
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("SyncRockerWithServer->" + e.Message);
            }
        }

        public void SyncSysStatusWithServer()
        {
            try
            {
                CurrentContext.SysCache.SystemRealTimeStatus.CurrStatus = CurrentContext.Status;
                WsClient.Instance.Send(CurrentContext.SysCache?.SystemRealTimeStatus.CurrStatus.ToString(), "syncStatus");
            }
            catch (Exception e)
            {
                LogFactory.Create().Warnning("SyncSysStatusWithServer->" + e.Message);
            }
        }

        public void SyncGasWithServer()
        {
            //SocketClienct.Instance.Send(new { strength = p.TimeInterval, flowrate = p.Flowrate, sensorId = data.DeviceId }, "syncGas");
        }
        #endregion
    }
}
