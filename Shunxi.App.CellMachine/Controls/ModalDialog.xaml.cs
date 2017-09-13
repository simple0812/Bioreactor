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
        ContentPresenter logo;
        public ModalDialog()
        {
            InitializeComponent();
            logo = (ContentPresenter)GetTemplateChild("logo");
        }

        public object TitleLogo
        {
            get { return (object)GetValue(TitleLogoProperty); }
            set { SetValue(TitleLogoProperty, value); }
        }

        public static readonly DependencyProperty TitleLogoProperty =
            DependencyProperty.Register("TitleLogo", typeof(object), typeof(ModalDialog),
                new PropertyMetadata(null));

    }
}
