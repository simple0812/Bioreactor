using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Windows.Threading;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    public class RockerViewModel : DeviceEditViewModel<Rocker>
    {
        public override string ViewName => "TemperatureEditView";

        public BaseDevice GetEntity()
        {
            return Entity;
        }

        public RockerViewModel(Rocker device) : base(device)
        {

        }
    }
}
