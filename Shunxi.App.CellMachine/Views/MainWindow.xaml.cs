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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Events;
using Prism.Regions;
using Shunxi.App.CellMachine.Common;
using Shunxi.App.CellMachine.Common.Behaviors;
using Shunxi.App.CellMachine.ViewModels;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models.cache;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.App.CellMachine.Views
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private IEventAggregator _ea;
        public MainWindow(IEventAggregator ea)
        {
            _ea = ea;
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            tbExpand.Visibility = this.IsFullscreen() ? Visibility.Collapsed : Visibility.Visible;
            tbCompress.Visibility = this.IsFullscreen() ? Visibility.Visible : Visibility.Collapsed;
            WsClient.Instance.SaveScheduleHandler += Instance_SaveScheduleHandler;
            WsClient.Instance.ControlHandler += Instance_ControlHandler;
        }

        private void Instance_SaveScheduleHandler(object obj)
        {
            Dispatcher.Invoke(() =>
            {
                var vm = this.DataContext as MainWindowViewModel;
                if (vm == null) return;

                var p = obj as SystemIntegration;
                if (p == null) return;

                var parameters = new NavigationParameters();
                parameters.Add("data", p);

                vm.Navigate("DevicesStatus", parameters);
            });
        }

        private void Instance_ControlHandler(string cmd, object paras)
        {
            Dispatcher.Invoke(() =>
            {
                _ea.GetEvent<CommandMessageEvent>().Publish(cmd);
//                switch (arg1)
//                {
//                    case "pause":
//                        ControlCenter.Instance.Pause().IgnorCompletion();
//                        break;
//                    case "start":
//                        break;
//                    case "restart":
//                        break;
//                    case "qr":
//                        LogFactory.Create().Info(arg2.ToString());
//                        var data = arg2.ToString().Split(",".ToCharArray());
//                        break;
//
//                    default:
//                        break;
//                }
            });
        }

        private void TbExpand_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.GoFullscreen();
            tbExpand.Visibility = Visibility.Collapsed;
            tbCompress.Visibility = Visibility.Visible;
        }

        private void TbCompress_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Application.Current.MainWindow.ExitFullscreen();
            tbExpand.Visibility = Visibility.Visible;
            tbCompress.Visibility = Visibility.Collapsed;
        }
    }
}
