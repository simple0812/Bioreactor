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
