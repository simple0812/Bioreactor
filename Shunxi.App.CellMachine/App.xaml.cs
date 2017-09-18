﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Common.Log;
using Shunxi.DataAccess;
using Shunxi.Infrastructure.Common.Configuration;
using ThicknessConverter = Xceed.Wpf.DataGrid.Converters.ThicknessConverter;

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
            base.OnStartup(e);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();

            var _regionManager = bootstrapper.Container.Resolve<IRegionManager>();
            _regionManager?.RequestNavigate("ContentRegion", "Index");

            Config.DetectorId = 0x01;
            using (var ctx = new IotContext())
            {
                ctx.Database.Exists();
            }
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