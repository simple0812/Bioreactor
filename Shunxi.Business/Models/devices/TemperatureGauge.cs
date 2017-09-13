using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class TemperatureGauge : BaseDevice
    {
        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Temperature;

        private CapacityLevel _capacityLevel = CapacityLevel.L500;
        public CapacityLevel Level
        {
            get { return _capacityLevel; }
            set
            {
                _capacityLevel = value;
                OnPropertyChanged();
            }
        }

        private double _temperature = 0;
        public double Temperature
        {

            get => _temperature;
            set
            {
                _temperature = value;
                OnPropertyChanged();
            }
        }

        private double _heaterTemperature = 0;
        public double HeaterTemperature
        {

            get => _heaterTemperature;
            set
            {
                _heaterTemperature = value;
                OnPropertyChanged();
            }
        }

        private double _envTemperature = 0;
        public double EnvTemperature
        {

            get => _envTemperature; set
            {
                _envTemperature = value;
                OnPropertyChanged();
            }
        }

        public override bool Validate(ref string msg)
        {
            if (Temperature < 0 || Temperature > 100)
            {
                msg = "温度取值范围为0~100";
                return false;
            }

            return base.Validate(ref msg);
        }
    }
}
