using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Shunxi.Business.Enums;
using Shunxi.Business.Models.cache;
using Shunxi.Business.Models.devices;
using Shunxi.Business.Protocols.Directives;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Protocols.SimDirectives;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;

namespace Shunxi.App.CellMachine.ViewModels
{
    public class IndexViewModel : BindableBase, INavigationAware
    {

        #region 属性

        private ObservableCollection<BaseDevice> _Entities;
        public ObservableCollection<BaseDevice> Entities
        {
            get => _Entities;
            set => SetProperty(ref _Entities, value);
        }

        private ObservableCollection<COMPortInfo> _comPorts;
        public ObservableCollection<COMPortInfo> ComPorts
        {
            get => _comPorts;
            set => SetProperty(ref _comPorts, value);
        }

        public ICommand CheckCommand { get; set; }
        public ICommand SelectCommand { get; set; }

        private string _CheckInfo;
        public string CheckInfo
        {
            get => _CheckInfo;
            set => SetProperty(ref _CheckInfo, value);
        }

        private bool _CanCheck;
        public bool CanCheck
        {
            get => _CanCheck;
            set => SetProperty(ref _CanCheck, value);
        }

        public string QrCode => $"http://{Config.SERVER_ADDR}:{Config.SERVER_PORT}/api/iot/qrcode?deviceid=" + Shunxi.Common.Utility.Common.GetUniqueId();
        #endregion

        private void CheckHandle()
        {
            ShowBusyDialog(true);
            SendCheckDirectives();
        }
        public IndexViewModel()
        {
            CheckCommand = new DelegateCommand(CheckHandle);
            ComPorts = new ObservableCollection<COMPortInfo>(COMPortInfo.GetCOMPortsInfo());

            var pumpIn = new Pump()
            {
                DeviceId = Config.Pump1Id,
                Name = Config.Pump1Name,
                IsEnabled = false,
            };

            var pumpOut = new Pump()
            {
                DeviceId = Config.Pump3Id,
                Name = Config.Pump3Name,
                IsEnabled = false,
            };
            var tempe = new TemperatureGauge()
            {
                DeviceId = Config.TemperatureId,
                IsEnabled = false,
                Name = "Temperature"
            };
            var rocker = new Rocker()
            {
                DeviceId = Config.RockerId,
                IsEnabled = false,
                Name = "Rocker"
            };
            var gas = new Gas()
            {
                DeviceId = Config.GasId,
                IsEnabled = false,
                Name = "Gas"
            };
            var ph = new PhDevice()
            {
                DeviceId = Config.PhId,
                IsEnabled = false,
                Name = "Ph"
            };
            var xdo = new DoDevice()
            {
                DeviceId = Config.DoId,
                IsEnabled = false,
                Name = "Do",
            };
            Entities = new ObservableCollection<BaseDevice>()
            {
                pumpIn,
                pumpOut,
                tempe,
                rocker,
                gas,
                ph,
                xdo
            };
        }

        public async void OnNavigatedTo(NavigationContext navigationContext)
        {
            ShowBusyDialog(true);
            //await new UsbSerial().Build();
//            await QrCodeWorker.Instance.Init();
//            QrCodeWorker.Instance.Serial.ReceiveHandler += Helper_ReceiveHandler;

            if (CurrentContext.Status != SysStatusEnum.Unknown
                && CurrentContext.Status != SysStatusEnum.Discarded
                && CurrentContext.Status != SysStatusEnum.Ready
                && CurrentContext.Status != SysStatusEnum.Completed)
            {
                CanCheck = false;
                ShowBusyDialog(false);
                return;
            }

            DirectiveWorker.Instance.SetIsRtry(false);
            DirectiveWorker.Instance.SerialPortEvent += Instance_SerialPortEvent;

            if (Entities.All(each => each.IsEnabled))
            {
                ShowBusyDialog(false);
                return;
            }
            ;
            SendCheckDirectives();
        }

        private void Helper_ReceiveHandler(byte[] obj)
        {
            var url = Encoding.UTF8.GetString(obj).Trim();
            if (url.IndexOf("http://", StringComparison.Ordinal) != 0)
            {
                return;
            }

            url += "&deviceid=" + Shunxi.Common.Utility.Common.GetUniqueId();

            SimWorker.Instance.Enqueue(new HttpCompositeDirective(url, p =>
            {
                //{"code":"success","message":"","result":{"id":3,"customerId":null,"serialNumber":"123456abcd","description":null,"type":9,"name":"通用耗材","status":1,"createdAt":1494557202,"usedTimes":3}}
                //{"code":"error","message":"耗材类型与设备类型不匹配","result":""}
                LogFactory.Create().Info("HTTP END");
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    try
                    {
                        var t = JObject.Parse(p.Code.ToString());

                        var code = t["code"].ToString();
                        var message = t["message"].ToString();

//                        if (code != "success")
//                        {
//                            Consumable.ConsumableName = "";
//                            Consumable.ConsumableSerialNumber = "";
//                            Consumable.ConsumableUsedTimes = 0;
//                            ScanErrorMsg = message;
//                            return;
//                        }

                        var result = JObject.Parse(t["result"].ToString());
                        var serialNumber = result["serialNumber"].ToString();
                        var name = result["name"].ToString();
                        var description = result["description"].ToString();
                        var type = result["type"].ToString();
                        int usedTimes = int.TryParse(result["usedTimes"].ToString(), out usedTimes) ? usedTimes : 0;

//                        ScanErrorMsg = "";
//                        Consumable.ConsumableName = name;
//                        Consumable.ConsumableSerialNumber = serialNumber;
//                        Consumable.ConsumableUsedTimes = usedTimes;
                    }
                    catch (Exception)
                    {
                        LogFactory.Create().Info("parse http response error");
                    }
                });
            }));

        }


        private void ShowBusyDialog(bool isShow)
        {
            App.BC.IsBusy = isShow;
        }
        private void Instance_SerialPortEvent(SerialPortEventArgs args)
        {
            if (!args.IsSucceed)
            {
                int id = int.TryParse(args.Message, out id) ? id : -1;

                var device = Entities.FirstOrDefault(each => each.DeviceId == id);

                device.IsRunning = false;
                device.IsChecked = true;
                device.IsEnabled = false;
            }
            else
            {
                var device = Entities.FirstOrDefault(p => p.DeviceId == args.Result.Data.DeviceId);
                if (device == null) return;
                device.IsRunning = false;
                device.IsChecked = true;
                device.IsEnabled = true;
            }

            if (!args.IsSucceed)
            {
                int id = int.TryParse(args.Message, out id) ? id : -1;

                var device = Entities.FirstOrDefault(each => each.DeviceId == id);

                device.IsRunning = false;
                device.IsChecked = true;
                device.IsEnabled = false;
            }
            else
            {
                var device = Entities.FirstOrDefault(p => p.DeviceId == args.Result.Data.DeviceId);
                if (device == null) return;
                device.IsRunning = false;
                device.IsChecked = true;
                device.IsEnabled = true;
            }

            //所有设备都检测完毕
            if (Entities.All(p => p.IsChecked))
            {
                ShowBusyDialog(false);
                CanCheck = true;
                CheckInfo = Entities.All(p => p.IsEnabled) ? "设备正常，请扫描二维码添加耗材" : "设备异常，请重试或者检查设备";
            }
        }

        private void SendCheckDirectives()
        {

            foreach (var each in Entities)
            {
                each.IsChecked = false;
            }
            CanCheck = false;

            Dispatcher.CurrentDispatcher.Invoke( async () =>
            {
                foreach (var each in Entities)
                {
                    each.IsRunning = true;
                    await Task.Delay(500);
                    DirectiveWorker.Instance.PrepareDirective(new IdleDirective(each.DeviceId, each.DeviceType));
                }
            });
        }
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            DirectiveWorker.Instance.SetIsRtry(true);
            DirectiveWorker.Instance.SerialPortEvent -= Instance_SerialPortEvent;
            QrCodeWorker.Instance.Serial.ReceiveHandler -= Helper_ReceiveHandler;
        }
    }
}
