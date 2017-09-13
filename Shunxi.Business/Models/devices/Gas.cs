using Shunxi.Business.Enums;

namespace Shunxi.Business.Models.devices
{
    public class Gas : BaseDevice
    {
        public override TargetDeviceTypeEnum DeviceType => TargetDeviceTypeEnum.Gas;
        private double _concentration = 0;
        public double Concentration
        {

            get { return _concentration; }
            set
            {
                _concentration = value;
                OnPropertyChanged();
            }
        }

        private double _flowRate = 0;
        public double FlowRate
        {

            get { return _flowRate; }
            set
            {
                _flowRate = value;
                OnPropertyChanged();
            }
        }

        public override bool Validate(ref string msg)
        {
            if (Concentration < 0 || Concentration > 20)
            {
                msg = "浓度取值范围为0~20";
                return false;
            }
            if (FlowRate < 0 || FlowRate > 400)
            {
                msg = "流量取值范围为0~400";
                return false;
            }

            return base.Validate(ref msg);
        }
    }
}
