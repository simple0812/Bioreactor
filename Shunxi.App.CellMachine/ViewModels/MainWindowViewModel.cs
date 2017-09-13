
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.App.CellMachine.Common.Behaviors;

namespace Shunxi.App.CellMachine.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private readonly IRegionManager _regionManager;
        private string CurrPage = "";

        private string _title = "一次性细胞培养生物反应器";
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        public BusyCls BC => App.BC;

        public DelegateCommand<string> NavigateCommand { get; private set; }
        public DelegateCommand<object[]> SelectedCommand { get; private set; }

        public MainWindowViewModel(IRegionManager regionManager)
        {
            _regionManager = regionManager;

            NavigateCommand = new DelegateCommand<string>(Navigate);
            Entities = GetEntities();
            SelectedCommand = new DelegateCommand<object[]>(OnItemSelected);
        }

        private void OnItemSelected(object[] selectedItems)
        {
            if (selectedItems != null && selectedItems.Any())
            {
                var p = selectedItems.FirstOrDefault() as ModuleViewModelBase;
                if (p == null) return;

                SelectedEntity = p;

                Navigate(p.DocumentType);
            }
        }

        public void Navigate(string navigatePath)
        {
            if (navigatePath == null) return;

            _regionManager.RequestNavigate("ContentRegion", navigatePath, result =>
            {
                CurrPage = result.Context.Uri.OriginalString;
            });
        }


        ObservableCollection<ModuleViewModelBase> entities;
        public ObservableCollection<ModuleViewModelBase> Entities
        {
            get => entities;
            set => SetProperty(ref entities, value);
        }

        ModuleViewModelBase selectedEntity;
        public ModuleViewModelBase SelectedEntity
        {
            get { return selectedEntity; }
            set { SetProperty(ref selectedEntity, value); }
        }


        protected ObservableCollection<ModuleViewModelBase> GetEntities()
        {
            return new ObservableCollection<ModuleViewModelBase>() {
                new ModuleViewModelBase( "首页", "Index", this, "DevicesCheck", Color.FromArgb(0xFF, 0x00, 0x87, 0x9C)),
                new ModuleViewModelBase( "参数设置", "DevicesStatus", this, "DevicesSetting", Color.FromArgb(0xFF, 0xCC, 0x6D, 0x00)),
                new ModuleViewModelBase( "统计", "DataChart", this, "Chart", Color.FromArgb(0xFF, 0x40, 0x40, 0x40)),
                new ModuleViewModelBase( "培养记录", "HistoryRecord", this, "CultivateRecord", Color.FromArgb(0xFF, 0x00, 0x73, 0xC4))
            };
        }
    }

    public class ModuleViewModelBase
    {
        public ModuleViewModelBase() { }
        public ModuleViewModelBase(string title, string documentType, object parentViewModel, string icon, Color color)
        {
            Title = title;
            DocumentType = documentType;
            ParentViewModel = parentViewModel;
            Icon = $"/Assets/Images/{icon}.png";
            Color = color;
        }

        public string Icon { get; set; }
        public string Title { get; private set; }
        public string DocumentType { get; private set; }
        public object ParentViewModel { get; private set; }
        public Color Color { get; set; }
    }
}
