using Prism.Commands;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels.Devices
{
    public interface IEditViewModel
    {
         DelegateCommand SaveCommand { get;  }
         DelegateCommand SaveAndCloseCommand { get;  }
         DelegateCommand CancelAndCloseCommand { get; }
         string ViewName { get; }

        BaseDevice GetEntity();
    }
}
