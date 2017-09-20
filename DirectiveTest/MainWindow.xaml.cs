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
using Newtonsoft.Json;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;

namespace DirectiveTest
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DirectiveWorker.Instance.SetIsRtry(false);
            DirectiveWorker.Instance.SerialPortEvent += Worker_SerialPortEvent;
        }

        private void Worker_SerialPortEvent(SerialPortEventArgs args)
        {
            if (!args.IsSucceed) return;

            Dispatcher.Invoke(() =>
            {
                txtResult.Text = JsonConvert.SerializeObject(args.Result.Data);
            });

        }
        private TargetDeviceTypeEnum GetDeviceTypeById(int id)
        {
            TargetDeviceTypeEnum type = TargetDeviceTypeEnum.Unknown;
            switch (id)
            {
                case 1:
                case 2:
                case 3:
                case 4:
                    type = TargetDeviceTypeEnum.Pump;
                    break;
                case 0x80:
                    type = TargetDeviceTypeEnum.Rocker;
                    break;
                case 0xa0:
                case 0xa1:
                case 0x90:
                    type = TargetDeviceTypeEnum.Temperature;
                    break;
                case 0x91:
                case 0x92:
                    type = TargetDeviceTypeEnum.Gas;
                    break;
            }

            return type;
        }
        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var bi = cbId.SelectedItem as ComboBoxItem;

            if (bi == null) return;
            var btn = sender as Button;
            int id = int.TryParse(bi.Tag.ToString(), out id) ? id : 1;
            var type = GetDeviceTypeById(id);
            if (type == TargetDeviceTypeEnum.Unknown) return;

            int rate = int.TryParse(txtRate.Text, out rate) ? rate : 100;
            int angle = int.TryParse(txtAngel.Text, out angle) ? angle : 1;

            if (btn?.Content == null) return;
            try
            {
                switch (btn.Content.ToString())
                {
                    case "开始":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(id, rate, angle, (int)DirectionEnum.In, type));
                    }
                        break;

                    case "暂停":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(id, type));
                    }
                        break;


                    case "普通轮询":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new IdleDirective(id, type));
                    }
                        break;

                    case "开始后轮询":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new RunningDirective(id, type));
                    }
                        break;

                    case "暂停后轮询":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new PausingDirective(id, type));
                    }
                        break;
                    case "正转":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(id, rate, angle, (int)DirectionEnum.In, type));
                        break;
                    }
                    case "反转":
                    {
                        DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(id, rate, angle, (int)DirectionEnum.Out, type));
                        break;
                    }

                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                txtResult.Text = ex.Message;
            }
        }


        private void ButtonBasex_OnClick(object sender, RoutedEventArgs e)
        {
            if (DirectiveWorker.Instance._serialPort.Status == SerialPortStatus.Opened ||
                DirectiveWorker.Instance._serialPort.Status == SerialPortStatus.Opening)
            {
                tbPortStatus.Text = "串口已打开";
                return;
            }

            (DirectiveWorker.Instance._serialPort as UsbSerial)?.Open(txtPort.Text);
        }
    }
}
