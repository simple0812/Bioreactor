using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class DoDevice : BaseDevice
    {
        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Do;
        private double _do = 0;
        public double DO
        {

            get => _do;
            set
            {
                _do = value;
                OnPropertyChanged();
            }
        }
    }
}
