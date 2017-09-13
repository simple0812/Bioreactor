using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class Rocker : BaseDevice
    {
        public Rocker()
        {
            
        }
        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Rocker;
        private double _Speed = 0;
        public double Speed
        {

            get { return _Speed; }
            set
            {
                _Speed = value;
                OnPropertyChanged();
            }
        }

        private double _angle = 0;
        public double Angle
        {

            get { return _angle; }
            set
            {
                _angle = value;
                OnPropertyChanged();
            }
        }


        public override bool Validate(ref string msg)
        {
            if (Speed < 0 || Speed > 100)
            {
                msg = "转速取值范围为0~100";
                return false;
            }

            if (Angle < 0 || Angle > 24)
            {
                msg = "角度取值范围为0~24";
                return false;
            }

            return base.Validate(ref msg);
        }
    }
}
