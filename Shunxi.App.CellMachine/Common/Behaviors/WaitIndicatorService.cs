using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interactivity;
using System.Windows.Threading;
using Xceed.Wpf.Toolkit;
using ThicknessConverter = Xceed.Wpf.DataGrid.Converters.ThicknessConverter;

namespace Shunxi.App.CellMachine.Common.Behaviors
{
    public interface IWaitIndicatorService
    {
        void Activate(bool active);
    }

    public class WaitIndicatorService : Behavior<BusyIndicator>, IWaitIndicatorService
    {
        void IWaitIndicatorService.Activate(bool active)
        {
//            Dispatcher.CurrentDispatcher.Invoke(() =>
//            {
//                var p = Application.Current.MainWindow.FindName("BusyIndicator") as BusyIndicator;
//                if (p == null) return;
//
//                p.IsBusy = active;
//            });
        }
    }
}
