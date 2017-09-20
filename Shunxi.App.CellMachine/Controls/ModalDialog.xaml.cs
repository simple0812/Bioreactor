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

namespace Shunxi.App.CellMachine.Controls
{
    /// <summary>
    /// ModalDialog.xaml 的交互逻辑
    /// </summary>
    public partial class ModalDialog : Window
    {
        public ModalDialog()
        {
            InitializeComponent();
        }

        public void Show(string msg)
        {
            txtMsg.Text = msg;
            this.ShowDialog();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
