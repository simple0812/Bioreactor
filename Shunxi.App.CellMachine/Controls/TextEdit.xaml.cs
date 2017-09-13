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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Prism.Mvvm;

namespace Shunxi.App.CellMachine.Controls
{
    /// <summary>
    /// TextEdit.xaml 的交互逻辑
    /// </summary>
    public partial class TextEdit : UserControl
    {
        public string Header
        {
            get
            {
                var value = GetValue(HeaderProperty);
                return value?.ToString() ?? "";
            }
            set => SetValue(HeaderProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(string), typeof(TextEdit), null);

        public string EditValue
        {
            get
            {
                var value = GetValue(EditValueProperty);
                return value?.ToString() ?? "";
            }
            set => SetValue(EditValueProperty, value);
        }

        public static readonly DependencyProperty EditValueProperty =
            DependencyProperty.Register("EditValue", typeof(string), typeof(TextEdit), null);

        public TextEdit()
        {
            InitializeComponent();
        }
    }
}
