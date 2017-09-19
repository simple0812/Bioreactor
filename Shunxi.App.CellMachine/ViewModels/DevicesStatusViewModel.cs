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

        public string _Name;

        public string Name
        {
            get => _Name;
            set => SetProperty(ref _Name, value);
        }

        public CellCultivation _CellCultivation;

        public CellCultivation CellCultivation
        {
            get => _CellCultivation;
            set => SetProperty(ref _CellCultivation, value);
        }

        public string _CurrTime;

        public string CurrTime
        {
            get => _CurrTime;
            set => SetProperty(ref _CurrTime, value);
        }

        #endregion

        #region Command

        public ICommand EditCommand { get; set; }
        public ICommand EditCellCommand { get; set; }
        public ICommand PauseCommand { get; set; }
        public ICommand CancelCommand { get; set; }
        public ICommand StartCommand { get; set; }
        private bool canSave = true;

        private bool CanEdit(BaseDevice device)
        {
            return canSave;
        }

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

        public bool _CanStart;

        public bool CanStart
        {
            get => _CanStart;
            set => SetProperty(ref _CanStart, value);
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

        public bool _CanPause;

        public bool CanPause
        {
            get => _CanPause;
            set => SetProperty(ref _CanPause, value);
        }

        private void Pause()
        {
            LogFactory.Create().Info("start pause directive");
            ControlCenter.Instance.Pause().IgnorCompletion();
        }

        public bool _CanCancel;

        public bool CanCancel
        {
            get => _CanCancel;
            set => SetProperty(ref _CanCancel, value);
        }

        private void Cancel()
        {
        }

        #endregion

        public DevicesStatusViewModel()
        {
            EditCommand = new DelegateCommand<BaseDevice>(Edit);

            StartCommand = new DelegateCommand(Start);
            PauseCommand = new DelegateCommand(Pause);
            CancelCommand = new DelegateCommand(Cancel);
            EditCellCommand = new DelegateCommand(EditCell);
        }


        public void Init()
        {
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
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Init();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {

        }
    }
}
