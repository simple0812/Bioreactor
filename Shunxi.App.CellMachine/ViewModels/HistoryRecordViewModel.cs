using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Logic;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.ViewModels
{

    public class HistoryRecordViewModel : BindableBase, INavigationAware
    {
        public ObservableCollection<CellCultivation> _Entities;
        public ObservableCollection<CellCultivation> Entities
        {
            get => _Entities;
            set => SetProperty(ref _Entities, value);
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Entities = new ObservableCollection<CellCultivation>(CultivationService.GetCultivationsFromDb());
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
           
        }
    }
}
