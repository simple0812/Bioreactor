using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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
using Shunxi.Business.Enums;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;

namespace SimpleApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _xtimer;

        public MainWindow()
        {
            InitializeComponent();

            DirectiveWorker.Instance.SetIsRtry(false);
            DirectiveWorker.Instance.SerialPortEvent += Worker_SerialPortEvent;
        }

        private void Worker_SerialPortEvent(SerialPortEventArgs args)
        {
            if (!args.IsSucceed)
            {

                return;
            }

            Dispatcher.Invoke( () =>
            {
                var data = args.Result.Data;

                if (data.DeviceId == 0xa1)
                {
                    var p = data as TemperatureDirectiveData;
                    if (p != null)
                    {
                        txtTempa1.Text =
                            $"{data.DeviceId}-{data.DirectiveId}->t1:{p.CenterTemperature}, t2:{p.HeaterTemperature}, t3:{p.EnvTemperature}";
                    }
                }
                else if (data.DeviceId == 0xa0)
                {
                    var p = data as TemperatureDirectiveData;
                    if (p != null)
                    {
                        txtTempa0.Text =
                            $"{data.DeviceId}-{data.DirectiveId}->t1:{p.CenterTemperature}, t2:{p.HeaterTemperature}, t3:{p.EnvTemperature}";
                    }
                }
                else if (data.DeviceId == 0x90)
                {
                    var p = data as TemperatureDirectiveData;
                    if (p != null)
                    {
                        txtTemp90.Text =
                            $"{data.DeviceId}-{data.DirectiveId}->t1:{p.CenterTemperature}, t2:{p.HeaterTemperature}, t3:{p.EnvTemperature}";
                    }
                }
                else if (data.DeviceId == 0x91 || data.DeviceId == 0x92)
                {
                    var p = data as GasDirectiveData;
                    if (p == null) return;
                    txtGas.Text = $"{data.DeviceId}-{data.DirectiveId}->c:{p.Concentration}, v:{p.Flowrate}";
                }
                else if (data.DeviceId == 0x80)
                {
                    var p = data as RockerDirectiveData;
                    if (p == null) return;
                    txtRocker.Text = $"{data.DeviceId}-{data.DirectiveId}->speed:{p.Speed}";
                }
                else if (data.DeviceId == 0x01 || data.DeviceId == 0x03)
                {
                    var p = data as PumpDirectiveData;
                    if (p == null) return;
                    TextBlock tb = data.DeviceId == 0x01 ? txtPump1 : txtPump3;
                    tb.Text = $"{data.DeviceId}-{data.DirectiveId}->flowrate:{p.FlowRate}, a:{p.Addition}";
                }
            });
        }

        private bool isRocker;
        private bool isTemp;
        private bool isGas;
        private bool isPump1;
        private bool isPump3;
        private int devicesCount = 1;
        private int speed;
        private double temperature;
        private double con;
        private int flowrate;
        private int lv;

        private int pump1Flowrate;
        private int pump1Volume;

        private int pump3Flowrate;
        private int pump3Volume;

        private void BtnStart_OnClick(object sender, RoutedEventArgs e)
        {
            speed = int.TryParse(txtSpeed.Text, out speed) ? speed : 5;
            temperature = double.TryParse(txtTemperature.Text, out temperature) ? temperature : 37;
            con = double.TryParse(txtCon.Text, out con) ? con : 5.0;
            flowrate = int.TryParse(txtFlowrate.Text, out flowrate) ? flowrate : 400;
            lv = cbLv.SelectedIndex + 1;

            pump1Flowrate = int.TryParse(txtPump1Flowrate.Text, out pump1Flowrate) ? pump1Flowrate : 5;
            pump1Volume = int.TryParse(txtPump1Volume.Text, out pump1Volume) ? pump1Volume : 5;

            pump3Flowrate = int.TryParse(txtPump1Flowrate.Text, out pump1Flowrate) ? pump1Flowrate : 5;
            pump3Volume = int.TryParse(txtPump3Volume.Text, out pump3Volume) ? pump3Volume : 5;

            isRocker = cbRocker.IsChecked ?? false;
            isTemp = cbTemp.IsChecked ?? false;
            isGas = cbGas.IsChecked ?? false;
            isPump1 = cbPump1.IsChecked ?? false;
            isPump3 = cbPump3.IsChecked ?? false;
            devicesCount = (rb3.IsChecked ?? false) ? 3 : (rb2.IsChecked ?? false) ? 2 : 1;

            start();
        }

        private void start()
        {
            if (!isRocker && !isTemp && !isGas && !isPump1 && !isPump3) return;

            if (isRocker)
            {
                Shunxi.Infrastructure.Common.Configuration.Config.DetectorId = 0x80;
            }
            else if (isGas)
            {
                Shunxi.Infrastructure.Common.Configuration.Config.DetectorId = 0x91;

            }
            else if (isTemp)
            {
                Shunxi.Infrastructure.Common.Configuration.Config.DetectorId = 0x90;
            }
            else if (isPump1)
            {
                Shunxi.Infrastructure.Common.Configuration.Config.DetectorId = 0x01;
            }
            else if (isPump3)
            {
                Shunxi.Infrastructure.Common.Configuration.Config.DetectorId = 0x03;
            }

            if (isRocker)
                DirectiveWorker.Instance.PrepareDirective(
                    new TryStartDirective(0x80, speed, 8, 0, TargetDeviceTypeEnum.Rocker));
            if (isTemp)
                DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(0x90, temperature * 10,
                    temperature * 10, lv, TargetDeviceTypeEnum.Temperature));
            if (isGas)
                DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(0x91, flowrate, con * 10,
                    (int)DirectionEnum.Anticlockwise, TargetDeviceTypeEnum.Gas));

            if (isPump1)
                DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(0x01, pump1Flowrate, pump1Volume,
                    (int)DirectionEnum.Anticlockwise, TargetDeviceTypeEnum.Gas));

            if (isPump3)
                DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(0x03, pump3Flowrate, pump3Volume,
                    (int)DirectionEnum.Anticlockwise, TargetDeviceTypeEnum.Gas));

            _xtimer?.Dispose();

            _xtimer = new Timer((obj) =>
            {
                if (isRocker)
                    DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0x80, TargetDeviceTypeEnum.Rocker));
                if (isGas)
                    DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0x91, TargetDeviceTypeEnum.Gas));

                if (isPump1)
                    DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0x01));

                if (isPump3)
                    DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0x03));

                if (isTemp)
                {
                    if (devicesCount == 3)
                    {
                        DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0xa1,
                            TargetDeviceTypeEnum.Temperature));

                        DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0xa0,
                            TargetDeviceTypeEnum.Temperature));
                    }
                    else if (devicesCount == 2)
                    {
                        DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0xa0,
                            TargetDeviceTypeEnum.Temperature));
                    }

                    DirectiveWorker.Instance.PrepareDirective(new RunningDirective(0x90,
                        TargetDeviceTypeEnum.Temperature));
                }

            }, null, 1500, 1000);
        }

        private void BtnStop_OnTapped(object sender, RoutedEventArgs e)
        {
            _xtimer?.Dispose();
            DirectiveWorker.Instance.Clean();
            if (isRocker)
                DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(0x80, TargetDeviceTypeEnum.Rocker));
            if (isTemp)
                DirectiveWorker.Instance.PrepareDirective(
                    new TryPauseDirective(0x90, TargetDeviceTypeEnum.Temperature));
            if (isGas)
                DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(0x91, TargetDeviceTypeEnum.Gas));

            if (isPump1)
                DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(0x01));

            if (isPump3)
                DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(0x03));
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (DirectiveWorker.Instance._serialPort.Status == SerialPortStatus.Opened ||
                DirectiveWorker.Instance._serialPort.Status == SerialPortStatus.Opening)
            {
                tbPortStatus.Text = "串口已打开";
                return;
            }
            var serial = DirectiveWorker.Instance._serialPort as UsbSerial;
            if(serial == null) return;

            await serial.Open(txtPort.Text);
            if (serial.Status == SerialPortStatus.Opened)
            {
                tbPortStatus.Text = "串口已打开";
            }
            else
            {
                tbPortStatus.Text = "串口失败";
            }
        }
    }
}

