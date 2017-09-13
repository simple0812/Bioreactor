using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Prism.Regions;
using Shunxi.App.CellMachine.Common.Behaviors;
using Shunxi.App.CellMachine.ViewModels;

namespace Shunxi.App.CellMachine.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IRegionManager _regionManager;
        public MainWindow(IRegionManager regionManager)
        {
            _regionManager = regionManager;
            InitializeComponent();
            //regionManager.RegisterViewWithRegion("ContentRegion", typeof(Index));
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _regionManager.RequestNavigate("ContentRegion", "Index");
//            _regionManager.RegisterViewWithRegion("ContentRegion", typeof(Index));
        }
    }
}
