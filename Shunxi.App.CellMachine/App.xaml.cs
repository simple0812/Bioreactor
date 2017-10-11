using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Protocols.SimDirectives;
using Shunxi.Common.Log;
using Shunxi.DataAccess;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.App.CellMachine
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
        }

        public static　BusyCls BC = new BusyCls();

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message + e.Exception.Source + e.Exception.StackTrace);
            e.Handled = true;
        }
        protected override void OnStartup(StartupEventArgs e)
        {
            var processName = Assembly.GetExecutingAssembly().GetName().Name;
            int processCount = Process.GetProcessesByName(processName).Length;
            if (processCount > 1)
            {
                MessageBox.Show("程序运行中，请关闭后重试");
                Environment.Exit(-2);
            }

            base.OnStartup(e);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            var _regionManager = bootstrapper.Container.Resolve<IRegionManager>();
            _regionManager?.RequestNavigate("ContentRegion", "Index");

            Config.DetectorId = 0x01;

//            SimWorker.Instance.Enqueue(new LocationCompositeDirective(x =>
//            {
//                var cnetScans = x.Code as CnetScan;
//                if (cnetScans == null) return;
//                var url =
//                    $"http://{Config.SERVER_ADDR}:{Config.SERVER_PORT}/api/sim/location?mcc={cnetScans.MCC}&mnc={cnetScans.MNC}&lac={cnetScans.Lac}&ci={cnetScans.Cellid}&deviceid={Shunxi.Common.Utility.Common.GetLocalIpex()}";
//                SimWorker.Instance.Enqueue(new HttpCompositeDirective(url, p =>
//                {
//                    Debug.WriteLine("aaa------->" + p.Code);
//                }));
//            }));
            Task.Run(() =>
            {
                using (var ctx = new IotContext())
                {
                    ctx.Initialize();
                }
            });
        }

        private void OnAppStartup_UpdateThemeName(object sender, StartupEventArgs e)
        {

            DevExpress.Xpf.Core.ApplicationThemeHelper.UpdateApplicationThemeName();
        }
    }

    public class BusyCls : BindableBase
    {
        public bool isBusy = true;

        public bool IsBusy
        {
            get => isBusy;
            set => SetProperty(ref isBusy, value);
        }

    }
}
