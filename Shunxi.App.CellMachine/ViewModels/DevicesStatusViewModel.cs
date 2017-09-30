using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.App.CellMachine.Controls;
using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.App.CellMachine.ViewModels.Devices;
using Shunxi.App.CellMachine.Views.Devices;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.App.CellMachine.ViewModels
{
    public class DevicesStatusViewModel : BindableBase, INavigationAware
    {
        #region 属性

        private BaseDevice _selectedDevice;
        public BaseDevice SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        //保存用户设置的设备信息
        private ObservableCollection<BaseDevice> _Devices;
        public ObservableCollection<BaseDevice> Devices
        {
            get => _Devices;
            set => SetProperty(ref _Devices, value);
        }

        private string _RunningStatus;
        public string RunningStatus
        {
            get => _RunningStatus;
            set => SetProperty(ref _RunningStatus, value);
        }

        private string _Name;
        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        private CellCultivation _CellCultivation;
        public CellCultivation CellCultivation
        {
            get => _CellCultivation;
            set => SetProperty(ref _CellCultivation, value);
        }

        private string _CurrTime;
        public string CurrTime
        {
            get => _CurrTime;
            set => SetProperty(ref _CurrTime, value);
        }

        private string _CurrStatus = CurrentContext.Status.ToString();
        public string CurrStatus
        {
            get => _CurrStatus;
            set => SetProperty(ref _CurrStatus, value);
        }

        private Rocker _Rocker;
        public Rocker XRocker
        {
            get => _Rocker;
            set { _Rocker = value; RaisePropertyChanged(); }
        }

        private Gas _gas;
        public Gas XGas
        {
            get => _gas;
            set { _gas = value; RaisePropertyChanged(); }
        }

        private PumpRealTimeStatus _PumpIn;
        public PumpRealTimeStatus XPumpIn
        {
            get => _PumpIn;
            set { _PumpIn = value; RaisePropertyChanged(); }
        }

        private PumpRealTimeStatus _PumpOut;
        public PumpRealTimeStatus XPumpOut
        {
            get => _PumpOut;
            set { _PumpOut = value; RaisePropertyChanged(); }
        }

        private TemperatureGauge _temperature;
        public TemperatureGauge XTemperatureGauge
        {
            get => _temperature;
            set { _temperature = value; RaisePropertyChanged(); }
        }

        private bool btnManualVisibility;
        public bool BtnManualVisibility
        {
            get => btnManualVisibility;
            set { btnManualVisibility = value; RaisePropertyChanged(); }
        }

        private bool btnStartVisibility;
        public bool BtnStartVisibility
        {
            get => btnStartVisibility;
            set { btnStartVisibility = value; RaisePropertyChanged(); }
        }

        private bool btnPauseVisibility;
        public bool BtnPauseVisibility
        {
            get => btnPauseVisibility;
            set { btnPauseVisibility = value; RaisePropertyChanged(); }
        }

        #endregion

        #region Command

        public ICommand EditCommand { get; set; }
        public ICommand EditCellCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand RestartCommand { get; set; }
        public ICommand StartCommand { get; set; }

        private void Edit(BaseDevice device)
        {
            var view = GetViewByType(device.DeviceType);
            var p = view as IEditDeviceView;
            p?.ShowView(device);
        }

        public void EditCell()
        {
            var p = new EditCellCultivation();
            p.ShowView(CellCultivation);
        }

        private IEditViewModel GetViewModelByType(TargetDeviceTypeEnum enumType)
        {
            var type = Type.GetType($"Shunxi.App.CellMachine.ViewModels.Devices.{enumType}ViewModel");
            if (type == null) return null;
            return Activator.CreateInstance(type) as IEditViewModel;
        }

        private Window GetViewByType(TargetDeviceTypeEnum enumType)
        {
            var type = Type.GetType($"Shunxi.App.CellMachine.Views.Devices.Edit{enumType}");
            if (type == null) return null;
            return Activator.CreateInstance(type) as Window;
        }

        private async void Start()
        {
            LogFactory.Create().Info("start start directive");
            if (CurrentContext.Status != SysStatusEnum.Paused)
            {
               await ControlCenter.Instance.Start();
            }
            else
            {
                ControlCenter.Instance.ReStart().IgnorCompletion();
            }
        }

        private void Pause()
        {
            LogFactory.Create().Info("start pause directive");
            ControlCenter.Instance.Pause().IgnorCompletion();
        }


        private async void Restart()
        {
            if (MessageBox.Show("要中断正在运行的任务吗？点击Yes后该任务将不可恢复",
                    "警告", MessageBoxButton.YesNo) == MessageBoxResult.No)
            {
                return;
            }

            App.BC.IsBusy = true;

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
                DeviceService.InitData();
                Init();
                App.BC.IsBusy = false;
            }
        }

        public void Init()
        {
            RemoveHandler();
            AddHandler();

            if (CurrentContext.Status != SysStatusEnum.Running && CurrentContext.Status != SysStatusEnum.Paused)
            {
                CurrentContext.Status = SysStatusEnum.Ready;
                var p = CultivationService.GetDefaultCultivation();
                CurrentContext.SysCache = new SystemCache(p);
                Devices = new ObservableCollection<BaseDevice>(p.GetDevices());

                CurrentContext.SysCache.SystemRealTimeStatus.CurrStatus = CurrentContext.Status = SysStatusEnum.Ready;
            }
            else
            {
                Devices = new ObservableCollection<BaseDevice>(CurrentContext.SysCache.System.GetDevices());
            }

            SelectedDevice = Devices[0];
            CellCultivation = CurrentContext.SysCache.System.CellCultivation;

            if (CurrentContext.SysCache?.SystemRealTimeStatus != null)
            {
                XRocker = CurrentContext.SysCache.SystemRealTimeStatus.Rocker;
                XGas = CurrentContext.SysCache.SystemRealTimeStatus.Gas;
                XTemperatureGauge = CurrentContext.SysCache.SystemRealTimeStatus.Temperature;
                XPumpIn = CurrentContext.SysCache.SystemRealTimeStatus.In;
                XPumpOut = CurrentContext.SysCache.SystemRealTimeStatus.Out;
            }

            CurrStatus = CurrentContext.Status.ToString();
            SwitchCommandStatus(CurrentContext.Status);
        }

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

        #endregion

        public DevicesStatusViewModel()
        {
            EditCommand = new DelegateCommand<BaseDevice>(Edit);

            StartCommand = new DelegateCommand(Start);
            PauseCommand = new DelegateCommand(Pause);
            RestartCommand = new DelegateCommand(Restart);
            EditCellCommand = new DelegateCommand(EditCell);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Init();
        }

        private void Instance_ErrorEvent(object sender, CustomException e)
        {
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            RemoveHandler();
        }

        public void InstanceSystemStatusChangeEvent(object sender, RunStatusChangeEventArgs e)
        {
            LogFactory.Create().Info($"==================sys->{e.SysStatus}==================");

            SwitchCommandStatus(e.SysStatus);
            
            CurrStatus = e.SysStatus.ToString().ToLower();
            if (e.SysStatus == SysStatusEnum.Starting)
            {
                CurrentContext.SysCache?.SystemRealTimeStatus.In.ClonePropertiesTo(XPumpIn);
                CurrentContext.SysCache?.SystemRealTimeStatus.Out.ClonePropertiesTo(XPumpOut);
            }
            else if (e.SysStatus == SysStatusEnum.Completed)
            {
                //new ModalDialog().Show("细胞培养流程已完成");
                LogFactory.Create().Info("细胞培养流程已完成");
                //await ReStart();
            }
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

        private void HandleGasStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as GasDirectiveData;
            if (data == null) return;

            XGas.FlowRate = data.Flowrate;
            XGas.Concentration = data.Concentration;
        }

        private  void HandleSensorStatusChange(IoStatusChangeEventArgs e)
        {
            var feedback = e.Feedback;

            var data = feedback as TemperatureDirectiveData;
            if (data == null) return;

            CurrentContext.SysCache?.SystemRealTimeStatus.Temperature.ClonePropertiesTo(XTemperatureGauge);
        }

        private void HandleRockerStatusChange(IoStatusChangeEventArgs e)
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

            CurrentContext.SysCache?.SystemRealTimeStatus.Rocker.ClonePropertiesTo(XRocker);

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


            if (CurrentContext.SysCache?.SystemRealTimeStatus.In.DeviceId == deviceId)
            {
                CurrentContext.SysCache?.SystemRealTimeStatus.In.ClonePropertiesTo(XPumpIn);
            }
            else
            {
                CurrentContext.SysCache?.SystemRealTimeStatus.Out.ClonePropertiesTo(XPumpOut);
            }
        }

        private void SwitchCommandStatus(SysStatusEnum status)
        {
            BtnManualVisibility = status == SysStatusEnum.Ready;

            switch (status)
            {
                case SysStatusEnum.Unknown:
                case SysStatusEnum.Discarded:
                case SysStatusEnum.Ready:
                {
                    BtnStartVisibility = true;
                    BtnPauseVisibility = false;
                    break;
                }

                case SysStatusEnum.Starting:
                case SysStatusEnum.Completed:
                {
                    BtnStartVisibility = false;
                    BtnPauseVisibility = false;
                    break;
                }

                case SysStatusEnum.Paused:
                {
                    BtnStartVisibility = true;
                    BtnPauseVisibility = false;
                    break;
                }

                case SysStatusEnum.Running:
                {
                    BtnStartVisibility = false;
                    BtnPauseVisibility = true;
                    break;
                }
                default:
                    break;
            }
        }
    }
}
