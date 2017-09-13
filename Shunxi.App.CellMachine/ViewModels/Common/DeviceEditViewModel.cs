using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Common
{
    public class DeviceEditViewModel<TDevice> : BindableBase, INavigationAware where TDevice : ViewModel, new()
    {
        public virtual string ViewName => "";
        bool entityPropertyChanged;
        public bool isSaved = false;

        public DeviceEditViewModel(TDevice device)
        {
            if (device == null)
                device = new TDevice();

            Entity = Clone(device);
            SubscribePropertyChanged();
        }

        public bool _HasValidData;
        public bool HasValidData
        {
            get => _HasValidData;
            set => SetProperty(ref _HasValidData, value);
        }

        public string _ValidMsg;
        public string ValidMsg
        {
            get => _ValidMsg;
            set => SetProperty(ref _ValidMsg, value);
        }

        TDevice entity;
        public TDevice Entity
        {
            get { return entity; }
            set => SetProperty(ref entity, value);
        }


        bool canSaveEntity = false;
        public bool CanSaveEntity
        {
            get { return canSaveEntity; }
            set => SetProperty(ref canSaveEntity, value);
        }

        public TDevice Clone(TDevice from)
        {
            TDevice to = new TDevice();
            CloneProperties(from, to);
            return to;
        }

        protected virtual bool Validate(ref string msg)
        {
            var device = Entity as ViewModel;
            if (device == null) return false;
            return device.Validate(ref msg);
        }

        void CloneProperties(TDevice from, TDevice to)
        {
            foreach (var propertyInfo in typeof(TDevice).GetProperties().Where(pi => pi.Name != "IsCloned" && pi.CanWrite && pi.CanRead))
            {
                propertyInfo.SetValue(to, propertyInfo.GetValue(from));
            }
        }

        void SubscribePropertyChanged()
        {
            Entity.PropertyChanged += Entity_PropertyChanged;
        }
        void UnsubscribeEvents()
        {
            Entity.PropertyChanged -= Entity_PropertyChanged;

        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            //
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            UnsubscribeEvents();
        }

        void Entity_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string msg = "";
            entityPropertyChanged = true;
            HasValidData = Validate(ref msg);
            ValidMsg = msg;

            canSaveEntityChange();
        }

        void canSaveEntityChange()
        {
            Dispatcher.CurrentDispatcher.Invoke(() =>
            {
                CanSaveEntity = entityPropertyChanged && HasValidData;
            });
        }
    }
}
