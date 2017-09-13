using Shunxi.Business.Enums;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;

namespace Shunxi.Business.Models
{

    public class DirectiveData
    {
        public int DeviceId { get; set; }
        public int DirectiveId { get; set; }
        public DirectiveTypeEnum DirectiveType { get; set; }
        public TargetDeviceTypeEnum DeviceType { get; set; }
        public int TimeInterval { get; set; }
        public string Hint { get; set; }

        public int DeviceStatus { get; set; }

        public override string ToString()
        {
            return
                $"DeviceId:{DeviceId},DirectiveId:{DirectiveId},DirectiveType:{DirectiveType},DeviceType:{DeviceType},TimeInterval:{TimeInterval},DeviceStatus:{DeviceStatus}";
        }

        public virtual void UpdateUi(BaseDevice device)
        {
        }
    }

    public class PumpDirectiveData : DirectiveData
    {
        public double Addition { get; set; }
        public double FlowRate { get; set; }
        public DirectionEnum Direction { get; set; }

        public override string ToString()
        {
            return
                base.ToString() + $",Addition:{Addition},FlowRate:{FlowRate},Direction:{Direction}";
        }

        public override void UpdateUi(BaseDevice device)
        {
            base.UpdateUi(device);
            var p = device as PumpRealTimeStatus;
            if(p == null || CurrentContext.SysCache?.SystemRealTimeStatus == null) return;
            var from = device.DeviceId == CurrentContext.SysCache?.SystemRealTimeStatus.In.DeviceId
                ? CurrentContext.SysCache?.SystemRealTimeStatus.In
                : CurrentContext.SysCache?.SystemRealTimeStatus.Out;

            from.ClonePropertiesTo(p);
        }
    }

    public class RockerDirectiveData : DirectiveData
    {
        public double Angle { get; set; }
        public double Speed { get; set; }
        public double CenterTemperature { get; set; }
        public double HeaterTemperature { get; set; }
        public double EnvTemperature { get; set; }
        public RockEnum RockMode { get; set; }

        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"device{DeviceId}-{DirectiveId}-speed：{Speed}";
            return
                base.ToString() + $",angle:{Angle},speed:{Speed},RockMode:{RockMode}";
        }

        public override void UpdateUi(BaseDevice device)
        {
            base.UpdateUi(device);
            var p = device as Rocker;
            if (p == null || CurrentContext.SysCache?.SystemRealTimeStatus == null) return;
            var from = CurrentContext.SysCache.SystemRealTimeStatus.Rocker;
            from.ClonePropertiesTo(p);
        }
    }

    public class GasDirectiveData : DirectiveData
    {
        public double Flowrate { get; set; }
        public double Concentration { get; set; }

        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"device{DeviceId}-{DirectiveId}-concentration：{Concentration}%,flowrate{Flowrate}";
            return
                base.ToString() + $",flowrate:{Flowrate}, con:{Concentration}";
        }

        public override void UpdateUi(BaseDevice device)
        {
            base.UpdateUi(device);
            var p = device as Gas;
            if (p == null || CurrentContext.SysCache?.SystemRealTimeStatus == null) return;
            var from = CurrentContext.SysCache.SystemRealTimeStatus.Gas;
            from.ClonePropertiesTo(p);
        }
    }

    public class TemperatureDirectiveData : DirectiveData
    {
        public double Addition { get; set; }
        public double CenterTemperature { get; set; }
        public double HeaterTemperature { get; set; }
        public double EnvTemperature { get; set; }


        public override string ToString()
        {
            if (DirectiveType == DirectiveTypeEnum.Running)
                return
                    $"device{DeviceId}-{DirectiveId}-t1：{CenterTemperature},t2：{HeaterTemperature},t3：{EnvTemperature}";
            return
                base.ToString() + $",env:{EnvTemperature},center:{CenterTemperature},heater:{HeaterTemperature}";
        }

        public override void UpdateUi(BaseDevice device)
        {
            base.UpdateUi(device);
            var p = device as TemperatureGauge;
            if (p == null || CurrentContext.SysCache?.SystemRealTimeStatus == null) return;
            var from = CurrentContext.SysCache.SystemRealTimeStatus.Temperature;
            from.ClonePropertiesTo(p);
        }
    }

    public class DirectiveResult
    {
        public bool Status { get; set; }
        public DirectiveTypeEnum SourceDirectiveType { get; set; }
        public DirectiveData Data { get; set; }
    }
}
