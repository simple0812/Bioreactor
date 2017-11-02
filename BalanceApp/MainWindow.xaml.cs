using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shunxi.Business.Protocols;
using Shunxi.Business.Protocols.Helper;

namespace BalanceApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            BalanceWorker.Instance.SerialPortEvent += Instance_SerialPortEvent;
        }

        private void Instance_SerialPortEvent(double obj)
        {
            Dispatcher.Invoke(() =>
            {
                txt.Text = obj + "g";

                MessageHelper.SendMessageByProcess("SimpleApp", obj +"g");
            });
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            (BalanceWorker.Instance._serialPort as UsbSerial)?.Open(txtCom.Text);
        }
    }
}
