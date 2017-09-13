using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    public class PumpViewModel:DeviceEditViewModel<Pump>
    {
        public override string ViewName => "PumpEditView";
       
        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public PumpViewModel(Pump device) : base(device)
        {
        }
    }
}
