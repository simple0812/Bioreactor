using System;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Controls.Primitives;
using Shunxi.Business.Enums;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.App.CellMachine.Controls
{
    /// <summary>
    /// ManualSet.xaml 的交互逻辑
    /// </summary>
    public partial class ManualSet : Window
    {
        private readonly Timer gasTimer;
        private const int DEFAULT_FILLGAS = 3 * 60;
        private int fillGasSeconds;
        public ManualSet()
        {
            InitializeComponent();
            fillGasSeconds = DEFAULT_FILLGAS;
            DirectiveWorker.Instance.SetIsRtry(false);
            DirectiveWorker.Instance.SerialPortEvent += Instance_SerialPortEvent; ;
            this.Unloaded += ManualSet_Unloaded;
            this.Closing += ManualSet_Closing;
            gasTimer = new Timer(1000);
            gasTimer.Elapsed += GasTimer_Elapsed;
        }

        private void ManualSet_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (fillGasSeconds > 0 && fillGasSeconds != DEFAULT_FILLGAS)
            {
                txtRet.Text = "请停止充气后在关闭该窗口";
                e.Cancel = true;
            }
        }

        private void GasTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (fillGasSeconds <= 0)
            {
                StopGas();
                return;
            }

            fillGasSeconds -= 1;

            Dispatcher.Invoke(() =>
            {
                txtFillGas.Text = $"充气{fillGasSeconds}秒后停止";
            });
        }

        private void Instance_SerialPortEvent(SerialPortEventArgs args)
        {
            if(!args.Result.Status) return;
            Dispatcher.Invoke(() =>
            {
                if (args.Result.Data.DeviceId == Config.GasId)
                {
                    if (args.Result.Data.DirectiveType == DirectiveTypeEnum.TryStart)
                    {
                        fillGasSeconds = DEFAULT_FILLGAS -1;
                        txtFillGas.Text = $"充气{fillGasSeconds}秒后停止";
                        gasTimer.Start();
                    }
                    else if (args.Result.Data.DirectiveType == DirectiveTypeEnum.TryPause)
                    {
                        txtFillGas.Text = "充气停止";
                    }
                }
            });
        }

        private void ManualSet_Unloaded(object sender, RoutedEventArgs e)
        {
            gasTimer.Dispose();
            DirectiveWorker.Instance.SetIsRtry(true);
            DirectiveWorker.Instance.SerialPortEvent -= Instance_SerialPortEvent;
        }

        private void BtnFastFilling_OnClick(object sender, RoutedEventArgs e)
        {
            DirectiveWorker.Instance.PrepareDirective(new TryStartDirective(Config.GasId, 200, 50, 1, TargetDeviceTypeEnum.Gas));
        }

        private void StopGas()
        {
            gasTimer.Stop();
            fillGasSeconds = 0;
            DirectiveWorker.Instance.PrepareDirective(new TryPauseDirective(Config.GasId, TargetDeviceTypeEnum.Gas));
        }

        private void BtnStopFillGas_OnClick(object sender, RoutedEventArgs e)
        {
            StopGas();
        }
    }
}
