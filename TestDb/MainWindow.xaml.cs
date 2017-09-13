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
using Shunxi.DataAccess;

namespace TestDb
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            ModalWindow modalWindow = new ModalWindow();
            modalWindow.ShowDialog();
            Console.WriteLine(ModalWindow.myValue);
        }

        private void BtnDb_OnClick(object sender, RoutedEventArgs e)
        {
//            using (var ctx = new IotContext())
//            {
//                ctx.Users.Add(new User() {Name = "abcde"});
//                ctx.SaveChanges();
//            }
        }
    }
}
