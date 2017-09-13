using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    public class TemperatureViewModel : DeviceEditViewModel<TemperatureGauge>
    {
        public override string ViewName => "TemperatureEditView";

        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public TemperatureViewModel(TemperatureGauge device) : base(device)
        {

        }
    }
}
