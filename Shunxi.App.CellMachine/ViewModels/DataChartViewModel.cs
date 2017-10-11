using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Logic;
using Shunxi.DataAccess;


namespace Shunxi.App.CellMachine.ViewModels
{

    public class DataChartViewModel : BindableBase, INavigationAware
    {
        public ICommand SelectedChangeCommand { get; set; }
        private ObservableCollection<StatChart> _Devices;
        public ObservableCollection<StatChart> Devices
        {
            get => _Devices;
            set => SetProperty(ref _Devices, value); 
        }

        private bool _IsChart = true;
        public bool IsChart
        {
            get => _IsChart;
            set => SetProperty(ref _IsChart, value);
        }

        private StatChart _selectedDevice;
        public StatChart SelectedDevice
        {
            get => _selectedDevice;
            set => SetProperty(ref _selectedDevice, value);
        }

        public DataChartViewModel()
        {
            SelectedChangeCommand = new DelegateCommand(SelectedItemChanged);
            Devices = new ObservableCollection<StatChart>()
            {
                new StatChart("温度", true),
                new StatChart("气体浓度", true),
//                new StatChart("进液泵", false),
//                new StatChart("出液泵", false),
                new StatChart("PH", true),
                new StatChart("DO", true),
            };

            SelectedDevice = Devices.FirstOrDefault();
        }

        public void SelectedItemChanged()
        {
            Series?.Clear();
            IsBusy = true;
            Task.Run(() =>
            {
                Series = new ObservableCollection<NumericPoint>(SelectedDevice.GetSeriesPoints());
                IsChart = SelectedDevice.IsChart;
                IsBusy = false;
            });
        }

        public ObservableCollection<NumericPoint> _Series;
        public ObservableCollection<NumericPoint> Series
        {
            get => _Series;
            set => SetProperty(ref _Series, value);
        }

        public bool _isBusy = true;
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            SelectedItemChanged();
        }

        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Series.Clear();
        }
    }

    public class NumericPoint
    {
        public double Argument { get; private set; }
        public double Value { get; private set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        public NumericPoint(double argument, double value)
        {
            this.Argument = argument;
            this.Value = value;
        }

        public NumericPoint(DateTime stime, DateTime etime, double value)
        {
            StartTime = stime;
            EndTime = etime;
            Value = value;
        }
    }

    public class StatChart
    {
        public string Name { get; set; }
        public bool IsChart { get; private set; }

        public StatChart(string name, bool isChart)
        {
            this.Name = name;
            IsChart = isChart;
        }

        public List<NumericPoint> GetSeriesPoints()
        {
            var lastId = CultivationService.GetLastCultivationId();
            using (var ctx = new IotContext())
            {
                IList<double> list = new List<double>();
                switch (Name)
                {
                    case "温度":
                        list = ctx.TemperatureRecords.Where(doc => doc.CellCultivationId == lastId).ToList().Select(each => each.Temperature)
                            .ToList(); break;
                    case "气体浓度":
                        list = ctx.GasRecords.Where(doc => doc.CellCultivationId == lastId).ToList().Select(each => each.Concentration)
                            .ToList(); break;
                    case "进液泵":
                        return ctx.PumpRecords.Where(each => each.DeviceId == 1 && each.CellCultivationId == lastId).ToList().Select(each =>
                                new NumericPoint(each.StartTime, each.EndTime, each.Volume))
                            .ToList();
                    case "出液泵":
                        return ctx.PumpRecords.Where(each => each.DeviceId == 3 && each.CellCultivationId == lastId).ToList().Select(each =>
                                new NumericPoint(each.StartTime, each.EndTime, each.Volume))
                            .ToList();
                    case "PH":
                        return new List<NumericPoint>();
                    case "DO":
                        return new List<NumericPoint>();
                }

                List < NumericPoint > xLIst = new List<NumericPoint>();
                for (var i = 0; i < list.Count; i++)
                {
                    xLIst.Add(new NumericPoint(i,list[i]));
                }

                return xLIst;
            }
        }
    }

}
