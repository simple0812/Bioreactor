using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Prism.Modularity;
using Prism.Regions;
using Prism.Unity;
using Shunxi.App.CellMachine.Common.Behaviors;
using Shunxi.App.CellMachine.Views;

namespace Shunxi.App.CellMachine
{
    public class ViewsModule : IModule
    {
        IRegionManager _regionManager;
        IUnityContainer _container;

        public ViewsModule(RegionManager regionManager, IUnityContainer container)
        {
            _regionManager = regionManager;
            _container = container;
        }

        public void Initialize()
        {
            _container.RegisterTypeForNavigation<Index>();
            _container.RegisterTypeForNavigation<DevicesStatus>();
            _container.RegisterTypeForNavigation<DataChart>();
            _container.RegisterTypeForNavigation<HistoryRecord>();
        }
    }
}
