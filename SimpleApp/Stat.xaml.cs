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
using LiveCharts;
using LiveCharts.Geared;
using Shunxi.Business.Logic;
using Shunxi.DataAccess;

namespace SimpleApp
{
    /// <summary>
    /// Stat.xaml 的交互逻辑
    /// </summary>
    public partial class Stat : Window
    {
        IList<double> temps = new List<double>();
        IList<double> cons = new List<double>();
        public Stat()
        {
            InitializeComponent();
            this.Loaded += Stat_Loaded;
            this.Unloaded += Stat_Unloaded;
        }

        private void Stat_Unloaded(object sender, RoutedEventArgs e)
        {
            cc.Series?.Clear();
        }

        private void Stat_Loaded(object sender, RoutedEventArgs e)
        {
            init();

            var xseries = new GLineSeries
            {
                Values = temps.AsGearedValues(),
                StrokeThickness = 1,
                PointGeometry = null
            };

            var pseries = new GLineSeries
            {
                Values = cons.AsGearedValues(),
                StrokeThickness = 1,
                PointGeometry = null
            };
            var temp = new SeriesCollection();


            temp.Add(xseries);
            temp.Add(pseries);

            cc.Series = temp;
        }

        private void init()
        {
            using (var ctx = new IotContext())
            {
                temps = ctx.TemperatureRecords.ToList().Select(each => each.Temperature)
                    .ToList();
                cons = ctx.GasRecords.ToList().Select(each => each.Concentration)
                    .ToList();
            }
        }

        public SeriesCollection GetSeriesPoints()
        {
            var temp = new SeriesCollection();
            using (var ctx = new IotContext())
            {
                var x = ctx.TemperatureRecords.ToList().Select(each => each.Temperature)
                    .ToList();
                var p = ctx.GasRecords.ToList().Select(each => each.Concentration)
                    .ToList();

                var xseries = new GLineSeries
                {
                    Values = x.AsGearedValues(),
                    StrokeThickness = 1,
                    PointGeometry = null
                };

                var pseries = new GLineSeries
                {
                    Values = p.AsGearedValues(),
                    StrokeThickness = 1,
                    PointGeometry = null
                };


                temp.Add(xseries);
                temp.Add(pseries);
            }

            return temp;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn == null) return;

            var tag = btn.Tag.ToString();

            var xseries = new GLineSeries
            {
                Values = temps.AsGearedValues(),
                StrokeThickness = 1,
                PointGeometry = null
            };

            var pseries = new GLineSeries
            {
                Values = cons.AsGearedValues(),
                StrokeThickness = 1,
                PointGeometry = null
            };

            var temp = new SeriesCollection();

            if (tag == "ALL" || tag == "TEMPERATURE")
            {
                temp.Add(xseries);
            }

            if (tag == "ALL" || tag == "GAS")
            {
                temp.Add(pseries);
            }

            cc.Series = temp;
        }
    }
}
