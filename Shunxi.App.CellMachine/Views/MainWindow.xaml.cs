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
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            WsClient.Instance.ControlHandler += Instance_ControlHandler;
            //WsClient.Instance.SaveScheduleHandler += Instance_SaveScheduleHandler;
        }

        private void Instance_SaveScheduleHandler(object obj)
        {
            var vm = this.DataContext as MainWindowViewModel;
            if(vm == null) return;

//            Dispatcher.Invoke(() =>
//            {
//                var p = obj as SystemIntegration;
//                if (p == null) return;
//
//                RemoveHandler();
//                AddHandler();
//
//                CurrentContext.Status = SysStatusEnum.Ready;
//                CurrentContext.SysCache = new SystemCache(p);
//                CurrentContext.SysCache.RunningState.CurrStatus = CurrentContext.Status = SysStatusEnum.Ready;
//                InitControl();
//            });

        }

        private void Instance_ControlHandler(string arg1, object arg2)
        {
            Dispatcher.Invoke(() =>
            {
                switch (arg1)
                {
                    case "pause":
                        ControlCenter.Instance.Pause().IgnorCompletion();
                        break;
                    case "start":
                        break;
                    case "restart":
                        break;
                    case "qr":
                        LogFactory.Create().Info(arg2.ToString());
                        var data = arg2.ToString().Split(",".ToCharArray());
                        break;

                    default:
                        break;
                }
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
