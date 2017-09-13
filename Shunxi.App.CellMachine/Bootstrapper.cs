using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Unity;
using Shunxi.App.CellMachine.Common.Behaviors;
using Shunxi.App.CellMachine.Views;

namespace Shunxi.App.CellMachine
{
    class Bootstrapper : UnityBootstrapper
    {
        protected override DependencyObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void InitializeShell()
        {
            Application.Current.MainWindow.Show();

        }

        protected override void ConfigureContainer()
        {
            base.ConfigureContainer();

//            Container.RegisterType<IWaitIndicatorService, WaitIndicatorService>();
        }

        protected override void ConfigureModuleCatalog()
        {
            var catalog = (ModuleCatalog)ModuleCatalog;
            catalog.AddModule(typeof(ViewsModule));
        }
    }
}
