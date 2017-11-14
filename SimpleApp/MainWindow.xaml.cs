﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic;
using Shunxi.Business.Models;
using Shunxi.Business.Protocols;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Tables;

namespace SimpleApp
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private Timer _xtimer;
        private Timer _warningTimer;
        private double currConcentration = 0;
        private double currTemperature = 0;

        public MainWindow()
        {
            InitializeComponent();

            DirectiveWorker.Instance.SetIsRtry(false);
            DirectiveWorker.Instance.SerialPortEvent += Worker_SerialPortEvent;
            this.Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var hwndSource = PresentationSource.FromVisual(this) as HwndSource;
            hwndSource?.AddHook(WndProc);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == MessageHelper.WM_COPYDATA)
            {
                var cds = (CopyDataStruct)System.Runtime.InteropServices.Marshal.PtrToStructure(lParam, typeof(CopyDataStruct));
                Debug.WriteLine("=======>" + cds.lpData + "," + cds.cbData);
            }
            return hwnd;
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
                        currTemperature = p.CenterTemperature;
                        txtTemp90.Text =
                            $"{data.DeviceId}-{data.DirectiveId}->t1:{p.CenterTemperature}, t2:{p.HeaterTemperature}, t3:{p.EnvTemperature}";
                        DeviceService.SaveTemperatureRecord(new TemperatureRecord()
                        {
                            CellCultivationId = CultivationService.GetLastCultivationId(),
                            Temperature = p.CenterTemperature,
                            CreatedAt = DateTime.Now
                        });
                    }
                }
                else if (data.DeviceId == 0x91 || data.DeviceId == 0x92)
                {
                    var p = data as GasDirectiveData;
                    if (p == null) return;
                    currConcentration = p.Concentration;
                    DeviceService.SaveGasRecord(new GasRecord()
                    {
                        CellCultivationId = CultivationService.GetLastCultivationId(),
                        Concentration = p.Concentration,
                        FlowRate = p.Flowrate,
                        CreatedAt = DateTime.Now
                    });
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
            Warning();
        }

        private void Warning()
        {
            _warningTimer?.Dispose();
            _warningTimer = new Timer(obj =>
            {
                Dispatcher.Invoke(() =>
                {
                    string err = "";
                    if (currConcentration > con * 1.2)
                    {
                        err = txtConError.Text = $"浓度过高 期望{con},实际{currConcentration} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
                    }
                    else if (currConcentration < con * 0.8)
                    {
                        err = txtConError.Text = $"浓度过低 期望{con},实际{currConcentration} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}\r\n";
                    }
                    else
                    {
                        err = txtConError.Text = "";
                    }

                    if (currTemperature > temperature * 1.2)
                    {
                        err += txtTempError.Text = $"温度过高 期望{temperature},实际{currTemperature} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    }
                    else if (currTemperature < temperature * 0.8)
                    {
                        err += txtTempError.Text = $"温度过低 期望{temperature},实际{currTemperature} at {DateTime.Now:yyyy-MM-dd HH:mm:ss}";
                    }
                    else
                    {
                        err += txtTempError.Text = "";
                    }

                    SendMail("lan.hu@sunscell.com", err);
                });

            }, null, TimeSpan.FromHours(2), TimeSpan.FromHours(2));
        }

        private bool SendMail(string to, string msg)
        {
            MailMessage message = new MailMessage();
            message.To.Add(to);
            message.From = new MailAddress("lei.zhang@sunscell.com", "lei", Encoding.UTF8);
            message.Subject = "生物反应器异常警报";
            message.SubjectEncoding = Encoding.UTF8;
            message.Body = msg;
            message.BodyEncoding = Encoding.UTF8;
            message.IsBodyHtml = false;
            message.Priority = MailPriority.Normal;
            // Attachment att = new Attachment("text.txt");//添加附件，确保路径正确
            // message.Attachments.Add(att);

            SmtpClient smtp = new SmtpClient();
            smtp.Credentials = new System.Net.NetworkCredential("lei.zhang@sunscell.com", "1qazxsw2!@QW");
            smtp.Host = "smtp.exmail.qq.com";
            object userState = message;

            try
            {
                smtp.SendAsync(message, userState);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Tips");
                return false;
            }
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
            _warningTimer?.Dispose();

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

        private void BtnStat_OnClick(object sender, RoutedEventArgs e)
        {
            new Stat().ShowDialog();
        }
    }
}

