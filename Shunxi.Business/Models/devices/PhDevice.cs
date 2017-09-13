using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class PhDevice : BaseDevice
    {
        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Ph;
        private double _ph = 0;
        public double PH
        {

            get => _ph;
            set
            {
                _ph = value;
                OnPropertyChanged();
            }
        }

        public override bool Validate(ref string msg)
        {
            if (PH < 0 || PH > 14)
            {
                msg = "PH取值范围为0~14";
                return false;
            }

            return base.Validate(ref msg);
        }
    }
}
