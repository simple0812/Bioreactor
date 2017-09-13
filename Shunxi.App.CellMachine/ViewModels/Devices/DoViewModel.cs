using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    class DoViewModel : DeviceEditViewModel<DoDevice>
    {
        public override string ViewName => "DoEditView";
        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public DoViewModel(DoDevice device) : base(device)
        {
        }
    }
}
