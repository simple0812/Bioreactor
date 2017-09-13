using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    public class GasViewModel : DeviceEditViewModel<Gas>
    {
        public override string ViewName => "GasEditView";
        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public GasViewModel(Gas device) : base(device)
        {
        }
    }

}
 