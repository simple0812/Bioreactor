using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    class PhViewModel : DeviceEditViewModel<PhDevice>
    {
        public override string ViewName => "PhEditView";
        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public PhViewModel(PhDevice device) : base(device)
        {
        }
    }
}
