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
using System.Windows.Shapes;
using Shunxi.App.CellMachine.ViewModels.Common;
using Shunxi.Business.Enums;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.Controls
{
    /// <summary>
    /// EditCellCultivation.xaml 的交互逻辑
    /// </summary>
    public partial class EditCellCultivation : Window
    {
        public EditCellCultivation()
        {
            InitializeComponent();
        }

        private DeviceEditViewModel<CellCultivation> vm;
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            if (vm != null)
                vm.isSaved = true;

            this.Close();
        }

        private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        public bool ShowView(CellCultivation device)
        {
            vm = new DeviceEditViewModel<CellCultivation>(device);
            this.DataContext = vm;
            this.ShowDialog();

            Debug.WriteLine("edit end");

            if (vm.isSaved)
            {
                if (CurrentContext.Status == SysStatusEnum.Ready || CurrentContext.Status == SysStatusEnum.Completed)
                {
                    device.Name = vm.Entity.Name;
                    device.UserName = vm.Entity.UserName;
                    device.Cell = vm.Entity.Cell;
                    device.Description = vm.Entity.Description;
                }

            }
            return vm.isSaved;
        }
    }
}
