using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using LiveCharts.Geared;
using LiveCharts.Wpf;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Logic;
using Shunxi.DataAccess;


namespace Shunxi.App.CellMachine.ViewModels
{

    public class DataChartViewModel : BindableBase, INavigationAware
    {
        private SeriesCollection _Series;
        public SeriesCollection Series
        {
            get => _Series;
            set => SetProperty(ref _Series, value);
        }

        private void initData()
        {
            Series = new SeriesCollection();
            using (var ctx = new IotContext())
            {
                var p = ctx.TemperatureRecords.ToList().Select(each => each.Temperature);
                var x = ctx.TemperatureRecords.ToList().Select(each => each.EnvTemperature);
                var t = ctx.TemperatureRecords.ToList().Select(each => each.HeaterTemperature);
                var gas = ctx.GasRecords.ToList().Select(each => each.Concentration);
                Series.Add(new StackedAreaSeries
                {
                    Title = "中心温度",
                    Values = new ChartValues<double>(p)
                });
                Series.Add(new StackedAreaSeries
                {
                    Title = "环境温度",
                    Values = new ChartValues<double>(x)
                });
                Series.Add(new StackedAreaSeries
                {
                    Title = "加热源温度",
                    Values = new ChartValues<double>(t)
                });
                Series.Add(new StackedAreaSeries
                {
                    Title = "气体浓度",
                    Values = new ChartValues<double>(gas)
                });
            }
        }


        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            initData();
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
