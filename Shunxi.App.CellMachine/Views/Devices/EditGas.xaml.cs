﻿using System;
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
using Shunxi.App.CellMachine.ViewModels.Devices;
using Shunxi.Business.Models.devices;

namespace Shunxi.App.CellMachine.Views.Devices
{
    /// <summary>
    /// EditGas.xaml 的交互逻辑
    /// </summary>
    public partial class EditGas : Window, IEditDeviceView
    {
        public EditGas()
        {
            InitializeComponent();
        }

        private GasViewModel vm;
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

        public bool ShowView(BaseDevice device)
        {
            vm = new GasViewModel(device as Gas);
            this.DataContext = vm;
            this.ShowDialog();

            Debug.WriteLine("edit end");

            if (vm.isSaved)
            {
                vm.Entity.ClonePropertiesTo(device);
            }
            return vm.isSaved;
        }
    }


}
