using System.Linq;
using System.Reflection;
using Newtonsoft.Json;
using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class BaseDevice : ViewModel
    {
        public virtual TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Unknown;

        #region 属性
        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private int _id;
        public int Id
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged();
            }
        }

        public int _cultivationId;
        public int CultivationId
        {
            get => _cultivationId;
            set
            {
                _cultivationId = value;
                OnPropertyChanged();
            }
        }

        public int _deviceId;
        public int DeviceId
        {
            get => _deviceId;
            set
            {
                _deviceId = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get => _isEnabled;
            set
            {
                _isEnabled = value;
                OnPropertyChanged();
            }
        }

        private bool _isRunning;
        public bool IsRunning
        {
            get => _isRunning;
            set
            {
                _isRunning = value;
                OnPropertyChanged();
            }
        }

        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public string _icon;
        public string Icon
        {
            get => _icon;
            set
            {
                _icon = value;
                OnPropertyChanged();
            }
        }
        #endregion


   

        public void ClonePropertiesTo(BaseDevice to)
        {
            foreach (var propertyInfo in this.GetType().GetProperties().Where(pi => pi.Name != "IsCloned" && pi.CanWrite && pi.CanRead))
            {
                propertyInfo.SetValue(to, propertyInfo.GetValue(this));
            }
        }

        public BaseDevice CloneProperties()
        {
            return JsonConvert.DeserializeObject(JsonConvert.SerializeObject(this), this.GetType()) as BaseDevice;
        }

    }
}
