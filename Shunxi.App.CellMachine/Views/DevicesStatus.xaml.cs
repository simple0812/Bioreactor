using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Shunxi.App.CellMachine.ViewModels;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Common.Log;

namespace Shunxi.App.CellMachine.Views
{
    /// <summary>
    /// DevicesStatus.xaml 的交互逻辑
    /// </summary>
    public partial class DevicesStatus : UserControl
    {
        private DispatcherTimer xTimer;
        private Storyboard sb, sbIn, sbOut;
        public DevicesStatus()
        {
            InitializeComponent();
            DataContext = new DevicesStatusViewModel();
            this.Loaded += DevicesStatus_Loaded; ;
            this.Unloaded += DevicesStatus_Unloaded; ;
            sb = Resources["sb"] as Storyboard;
            sbIn = Resources["sbIn"] as Storyboard;
            sbOut = Resources["sbOut"] as Storyboard;

            #region make storyboard is controllable
            //fixed Cannot perform action because the specified Storyboard was not applied to this object for interactive control.
            sb?.Begin(this, true);
            sb?.Stop(this);
            sbIn?.Begin(this, true);
            sbIn?.Stop(this);
            sbOut?.Begin(this, true);
            sbOut?.Stop(this);
            #endregion

            InitData();
            InitControl();
            SwitchCommandStatus(CurrentContext.Status);
        }

        private void DevicesStatus_Unloaded(object sender, RoutedEventArgs e)
        {
            RemoveHandler();
            if (xTimer == null) return;

            xTimer.Stop();
            xTimer.Tick -= XTimer_Tick;
        }

        private void DevicesStatus_Loaded(object sender, RoutedEventArgs e)
        {
            xTimer = new DispatcherTimer();
            xTimer.Interval = TimeSpan.FromSeconds(1);
            xTimer.Tick += XTimer_Tick;
            xTimer.Start();

            LogFactory.Create().Info("HomePage STATUS->" + CurrentContext.Status);

            InitData();
            InitControl();
            SwitchCommandStatus(CurrentContext.Status);
        }

        private void InitData()
        {
            RemoveHandler();
            AddHandler();
            SwitchCommandStatus(CurrentContext.Status);
        }

        #region 运行状态监控事件
        public async void InstanceSystemStatusChangeEvent(object sender, RunStatusChangeEventArgs e)
        {
            LogFactory.Create().Info($"==================sys->{e.SysStatus}==================");

            await this.Dispatcher.InvokeAsync(async () =>
            {
                SwitchCommandStatus(e.SysStatus);

                txtStatus.Text = e.SysStatus.ToString().ToLower();
                if (e.SysStatus == SysStatusEnum.Completed)
                {
                    var vm = DataContext as DevicesStatusViewModel;
                    if (vm != null)
                    {
                        await Task.Delay(1000);
                       // vm.NavigationService.Navigate("ChartView");

                    }
                }
            });
        }

        public void InstanceDeviceStatusChangeEvent(object sender, IoStatusChangeEventArgs e)
        {
            var deviceType = e.DeviceType;

            if (deviceType == TargetDeviceTypeEnum.Pump)
            {
                HandlePumpStatusChange(e);
            }
            else if (deviceType == TargetDeviceTypeEnum.Rocker)
            {
                HandleRockerStatusChange(e);
            }
            else if (deviceType == TargetDeviceTypeEnum.Temperature)
            {
                HandleSensorStatusChange(e);
            }
            else if (deviceType == TargetDeviceTypeEnum.Gas)
            {
                HandleGasStatusChange(e);
            }
        }

        private void Instance_ErrorEvent(object sender, CustomException e)
        {

        }
        #endregion

        #region 私有方法
        private void AddHandler()
        {
            ControlCenter.Instance.ErrorEvent += Instance_ErrorEvent;
            ControlCenter.Instance.DeviceStatusChangeEvent += InstanceDeviceStatusChangeEvent; ;
            ControlCenter.Instance.SystemStatusChangeEvent += InstanceSystemStatusChangeEvent; ;
        }

        private void RemoveHandler()
        {
            ControlCenter.Instance.ErrorEvent -= Instance_ErrorEvent;
            ControlCenter.Instance.DeviceStatusChangeEvent -= InstanceDeviceStatusChangeEvent; ;
            ControlCenter.Instance.SystemStatusChangeEvent -= InstanceSystemStatusChangeEvent; ;
        }

        private void XTimer_Tick(object sender, object e)
        {
            txtTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        }

        private void InitControl()
        {
            if (CurrentContext.SysCache?.SystemRealTimeStatus != null)
            {
                UpdatePumpIn();
                UpdatePumpOut();
                txtAngle.Text = CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Angle.ToString("0.0");
                txtRate.Text = CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Speed.ToString("0.0");
                if (CurrentContext.SysCache?.SystemRealTimeStatus.Rocker.IsRunning ?? false)
                {
                    sb.Begin(this, true);
                }
                else
                {
                    sb.Stop(this);
                }

                txtStatus.Text = CurrentContext.Status.ToString();
                txtEnvTemperature.Text = CurrentContext.SysCache.SystemRealTimeStatus.Temperature.EnvTemperature
                    .ToString("0.0");
                txtTemperature.Text =
                    CurrentContext.SysCache.SystemRealTimeStatus.Temperature.Temperature.ToString("0.0");
                txtHeaterTemperature.Text = CurrentContext.SysCache.SystemRealTimeStatus.Temperature.HeaterTemperature
                    .ToString("0.0");
                //                txtCultivationName.Text = CurrentContext.SysCache.System.CellCultivation.Name;
            }
        }


        private async void HandleGasStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as GasDirectiveData;
            if (data == null) return;
            await Dispatcher.InvokeAsync(() =>
            {
                txtFlowrate.Text = (data.Flowrate).ToString("0.0");
                txtCon.Text = (data.Concentration).ToString("0.0");
            });
        }

        private async void HandleSensorStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as TemperatureDirectiveData;
            if (data == null) return;

            await Dispatcher.InvokeAsync( () =>
            {
                txtTemperature.Text = (data.CenterTemperature).ToString("0.0");
                txtEnvTemperature.Text = (data.EnvTemperature).ToString("0.0");
                txtHeaterTemperature.Text = (data.HeaterTemperature).ToString("0.0");
            });
        }

        private async void HandleRockerStatusChange(IoStatusChangeEventArgs e)
        {
            var type = e.IoStatus;
            var feedback = e.Feedback;
            var data = feedback as RockerDirectiveData;

            if (data == null) return;

            if (CurrentContext.SysCache?.SystemRealTimeStatus == null)
            {
                LogFactory.Create().Warnning("RunningState is null when HandleRockerStatusChange");
                return;
            }

            await Dispatcher.InvokeAsync(() =>
            {
                txtRate.Text = CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Speed.ToString("0.0");
                txtAngle.Text = CurrentContext.SysCache.SystemRealTimeStatus.Rocker.Angle.ToString("0.0");
                if (CurrentContext.SysCache?.SystemRealTimeStatus.Rocker.IsRunning ?? false)
                {
                    if (GetAnimateStatus(sb) == ClockState.Stopped)
                            sb.Begin(this, true);
                }
                else
                {
                    if (GetAnimateStatus(sb) != ClockState.Stopped)
                        sb.Stop(this);
                }
            });
        }

        private ClockState GetAnimateStatus(Storyboard xsb)
        {
            try
            {
                return xsb.GetCurrentState(this);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            return ClockState.Stopped;
        }


        private void UpdatePumpIn()
        {

            if (CurrentContext.SysCache?.SystemRealTimeStatus.In == null) return;
            txtIn.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.RealTimeFlowRate.ToString("0.0");
            txtInVolume.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.CurrVolume.ToString("0.0");
            if (CurrentContext.SysCache?.SystemRealTimeStatus.In.IsRunning ?? false)
            {
                if (GetAnimateStatus(sbIn)  == ClockState.Stopped)
                    sbIn.Begin(this, true);
            }
            else
            {
                if (GetAnimateStatus(sbIn)  != ClockState.Stopped)
                    sbIn.Stop(this);
            }

            txtInRunTimes.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.RunTimes.ToString("0");
            txtInAllTimes.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.AllRunTimes.ToString("0");
            txtInStartTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.TheStartTime.ToString("yy-MM-dd HH:mm:ss");
            txtInEndTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.TheEndTime.ToString("yy-MM-dd HH:mm:ss");
            txtInNextTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.NextTime.ToString("yy-MM-dd HH:mm:ss");
            txtInFirstTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.FirstTime.ToString("yy-MM-dd HH:mm:ss");
            txtInLastTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.In.LastTime.ToString("yy-MM-dd HH:mm:ss");
        }

        private void UpdatePumpOut()
        {
            if (CurrentContext.SysCache?.SystemRealTimeStatus.Out == null) return;
            txtOut.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.RealTimeFlowRate.ToString("0.0");
            txtOutVolume.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.CurrVolume.ToString("0.0");
            if (CurrentContext.SysCache?.SystemRealTimeStatus.Out.IsRunning ?? false)
            {
                if (GetAnimateStatus(sbOut) == ClockState.Stopped)
                    sbOut.Begin(this,true);
            }
            else
            {
                if (GetAnimateStatus(sbOut) != ClockState.Stopped)
                    sbOut.Stop(this);
            }

            txtOutRunTimes.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.RunTimes.ToString("0");
            txtOutAllTimes.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.AllRunTimes.ToString("0");
            txtOutStartTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.TheStartTime.ToString("yy-MM-dd HH:mm:ss");
            txtOutEndTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.TheEndTime.ToString("yy-MM-dd HH:mm:ss");
            txtOutNextTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.NextTime.ToString("yy-MM-dd HH:mm:ss");
            txtOutFirstTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.FirstTime.ToString("yy-MM-dd HH:mm:ss");
            txtOutLastTime.Text = CurrentContext.SysCache?.SystemRealTimeStatus.Out.LastTime.ToString("yy-MM-dd HH:mm:ss");
        }

        private void HandlePumpStatusChange(IoStatusChangeEventArgs e)
        {
            var deviceId = e.DeviceId;
            var type = e.IoStatus;
            var feedback = e.Feedback;
            var data = feedback as PumpDirectiveData;
            if (data == null)
                return;

            if (CurrentContext.SysCache?.SystemRealTimeStatus == null)
            {
                LogFactory.Create().Warnning("RunningState is null when HandlePumpStatusChange");
                return;
            }

            Dispatcher.Invoke(() =>
            {
                if (CurrentContext.SysCache?.SystemRealTimeStatus.In.DeviceId == deviceId)
                {
                    UpdatePumpIn();
                }
                else
                {
                    UpdatePumpOut();
                }
            });
        }

        private void SwitchCommandStatus(SysStatusEnum status)
        {
            try
            {
                switch (status)
                {
                    case SysStatusEnum.Unknown:
                    case SysStatusEnum.Discarded:
                    case SysStatusEnum.Completed:
                    case SysStatusEnum.Ready:
                    {
                        btnStart.Visibility = Visibility.Visible;
                        btnPauseAll.Visibility = Visibility.Collapsed;
                        break;
                    }

                    case SysStatusEnum.Paused:
                    {
                        btnStart.Visibility = Visibility.Visible;
                        btnPauseAll.Visibility = Visibility.Collapsed;
                        break;
                    }

                    case SysStatusEnum.Running:
                    {
                        btnStart.Visibility = Visibility.Collapsed;
                        btnPauseAll.Visibility = Visibility.Visible;
                        break;
                    }
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                throw new CustomException(ex.Message, this.GetType().FullName);
            }
        }
        #endregion

        private async void BtnRestart_OnClick(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DevicesStatusViewModel;
//            if (vm != null)
//            {
//                var ret = await vm.MessageBoxService.ShowAsync("要中断正在运行的任务吗？点击Yes后该任务将不可恢复", "警告", MessageButton.YesNo);
//                if (ret == MessageResult.No) return;
//            }

            await ReStart();
        }

        private async Task ReStart()
        {
//            dialog = await CommonMsgDialog.InitAndShowAsync();
            RemoveHandler();
            try
            {
                LogFactory.Create().Info("start reset:" + CurrentContext.Status);
                await ControlCenter.Instance.Close();
            }
            catch (Exception e)
            {
                LogFactory.Create().Info("reset failed" + e.Message);
            }
            finally
            {
                await Dispatcher.InvokeAsync( () =>
                {
                    SwitchCommandStatus(CurrentContext.Status);
                    var vm = DataContext as DevicesStatusViewModel;
                    vm?.Init();
                    DeviceService.InitData();
                    InitData();
                    InitControl();

//                    dialog.Hide();
                });
            }
        }
    }
}
