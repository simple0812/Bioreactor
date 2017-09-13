using System;
using System.Threading;
using System.Threading.Tasks;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Controllers.Status;
using Shunxi.Business.Logic.Devices;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Common.Log;

namespace Shunxi.Business.Logic.Controllers
{
    public abstract class ControllerBase :IDisposable
    {
        //设备开始运行时间
        public DateTime StartTime { get; set; } = DateTime.MinValue;
        //设备停止运行时间(分为自动停止和强制停止)
        public DateTime StopTime { get; set; } = DateTime.MinValue;
        /*设备运行次数 会影响设备的下一次运行日期
         * if AlreadyRunTimes == 0 && starttime < DateTime.Now 则调整cultivation的starttime为当前时间。
         * 否则不做调整
        */
        public int AlreadyRunTimes { get; set; } = 0;

        protected Timer LoopTimer;

        public DeviceStatusEnum CurrentStatus { get; set; }
        public StatusBase Status;
        public virtual bool IsEnable => true;
        protected virtual int IdlePollingInterval => 10 * 1000;
        protected virtual int StartingPollingInterval => 500;
        protected virtual int RunningPollingInterval => 500;
        protected virtual int PausingPollingInterval => 500;
        //        protected AsyncManualResetEvent<DeviceIOResult> StartEvent = new AsyncManualResetEvent<DeviceIOResult>();
        //        protected AsyncManualResetEvent<DeviceIOResult> StopEvent = new AsyncManualResetEvent<DeviceIOResult>();
        protected TaskCompletionSource<DeviceIOResult> StartEvent = new TaskCompletionSource<DeviceIOResult>();
        protected TaskCompletionSource<DeviceIOResult> StopEvent = new TaskCompletionSource<DeviceIOResult>();

        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        public DeviceBase Device;
        public ControlCenter Center;

        protected ControllerBase(ControlCenter center, DeviceBase device)
        {
            SetStatus(DeviceStatusEnum.Idle);
            Device = device;
            Center = center;
        }

        public virtual void Cancel()
        {
            LoopTimer?.Dispose();
            CancellationTokenSource.Cancel();
            if (StartEvent.Task.Status != TaskStatus.Canceled && StartEvent.Task.Status != TaskStatus.Faulted &&
                StartEvent.Task.Status != TaskStatus.RanToCompletion)
            {
                StartEvent.TrySetResult(new DeviceIOResult(false, "CANCEL"));
            }
        }

        public void SetStatus(DeviceStatusEnum state)
        {
            this.CurrentStatus = state;

            switch (state)
            {
                case DeviceStatusEnum.Idle:
                    this.Status = new IdleStatus(this);
                    break;
                case DeviceStatusEnum.Error:
                    this.Status = new ErrorStatus(this);
                    break;
                case DeviceStatusEnum.Pausing:
                    this.Status = new PausingStatus(this);
                    break;
                case DeviceStatusEnum.PrePause:
                    this.Status = new PrePauseStatus(this);
                    break;
                case DeviceStatusEnum.PreStart:
                    this.Status = new PreStartStatus(this);
                    break;
                case DeviceStatusEnum.Running:
                    this.Status = new RunningStatus(this);
                    break;
                case DeviceStatusEnum.Startting:
                    this.Status = new StartingStatus(this);
                    break;
                default:
                    break;
            }
        }
        #region 控制命令

        public abstract Task<DeviceIOResult> Start();

        public virtual async Task<DeviceIOResult> ReStart()
        {
            if(!IsEnable || CurrentStatus == DeviceStatusEnum.AllFinished) return new DeviceIOResult(false, "DISABLED");
            CancellationTokenSource = new CancellationTokenSource();
            return await Start();
        }
        
        //设备停止
        public virtual async Task<DeviceIOResult> Stop()
        {
            if(!IsEnable || CurrentStatus == DeviceStatusEnum.AllFinished) return new DeviceIOResult(false, "DISABLED");

            SetStatus(DeviceStatusEnum.PrePause);
            Device.Stop();
            StopEvent = new TaskCompletionSource<DeviceIOResult>();

            return await StopEvent.Task;
        }

        //紧急关机 还未实现
        public abstract Task<DeviceIOResult> Close();
        
        //培养周期内 设备停止后需要处理轮询逻辑
        public virtual async Task<DeviceIOResult> Pause()
        {
            return await Stop();
        }
        #endregion

        #region 处理指令反馈
        public abstract void Next(SerialPortEventArgs args);

        protected void LoopHandle(Action action, int span = 0)
        {
            LoopTimer?.Dispose();
            if (!IsEnable) return;
            if (span < 0) span = 0;

            LoopTimer = new Timer((obj) =>
            {
                action.Invoke();
            }, null, span, -1);
        }

        public virtual void StartRunningLoop()
        {
            if (CurrentStatus != DeviceStatusEnum.Running && CurrentStatus != DeviceStatusEnum.Startting)
            {
                LogFactory.Create()
                    .Warnning(
                        $"device{Device.DeviceId} SysStatus is {CurrentStatus}, can not send Running Directive");
                return;
            }

            LoopHandle(Device.Running,  CurrentStatus == DeviceStatusEnum.Startting ? StartingPollingInterval : RunningPollingInterval);
        }

        public virtual void StartPauseLoop()
        {
            if (CurrentStatus != DeviceStatusEnum.Pausing)
            {
                LogFactory.Create()
                    .Warnning(
                        $"device{Device.DeviceId} SysStatus is {CurrentStatus}, can not send Pausing Directive");
                return;
            }

            LoopHandle(Device.Pausing, PausingPollingInterval);
        }

        public virtual void StartIdleLoop()
        {
            if (CurrentStatus != DeviceStatusEnum.Idle)
            {
                LogFactory.Create()
                    .Warnning(
                        $"device{Device.DeviceId} SysStatus is {CurrentStatus}, can not send Idle Directive");
                return;
            }

            LoopHandle(Device.Idle, IdlePollingInterval);
        }

        public abstract void ProcessRunningDirectiveResult(DirectiveData data, CommunicationEventArgs comEventArgs);
        public abstract void ProcessTryPauseResult(DirectiveData data, CommunicationEventArgs comEventArgs);
        public abstract void ProcessPausingResult(DirectiveData data, CommunicationEventArgs comEventArgs);
        public virtual void ProcessTryStartResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            SetStatus(DeviceStatusEnum.Startting);
            comEventArgs.DeviceStatus = DeviceStatusEnum.Startting;
            //记录泵的开始时间
            // 拿到TryStart反馈指令后启动running状态轮询
            OnCommunicationChange(comEventArgs);
            StartRunningLoop();
        }
        public virtual void ProcessIdleResult(DirectiveData data, CommunicationEventArgs comEventArgs)
        {
            if (CurrentStatus == DeviceStatusEnum.Idle)
            {
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
            }
            else
            {
                // 进入普通轮询状态
                SetStatus(DeviceStatusEnum.Idle);
                comEventArgs.DeviceStatus = DeviceStatusEnum.Idle;
            }

            StartIdleLoop();
        }

        #endregion

        #region 触发事件

        public virtual void OnCommunication(CommunicationEventArgs args)
        {
            if (CurrentContext.Status == SysStatusEnum.Starting)
            {
                Center.OnSystemStatusChange(new RunStatusChangeEventArgs() { SysStatus = SysStatusEnum.Running });
                Center.SyncSysStatusWithServer();
            }
        }

        public virtual void OnCommunicationChange(CommunicationEventArgs e)
        {
            if (e == null) return;
            LogFactory.Create().Info($"****************************{e.DeviceId}->{e.DeviceStatus}***********************");
        }

        public virtual void OnCustomError(CustomException obj)
        {
            Center.OnErrorEvent(obj);
            Dispose();
            CurrentContext.Status = SysStatusEnum.Unknown;
        }

        #endregion

        public void Dispose()
        {
            Cancel();
        }

    }
}
