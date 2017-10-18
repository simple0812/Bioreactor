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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using Shunxi.Business.Protocols;
using Shunxi.Business.Protocols.Helper;
using Shunxi.Business.Protocols.SimDirectives;
using Shunxi.Common.Log;

namespace Shunxi.App.CellMachine.Views
{
    /// <summary>
    /// Index.xaml 的交互逻辑
    /// </summary>
    public partial class Index : UserControl
    {
        public Index()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var p = cbCom.SelectedItem as COMPortInfo;
            if(p == null) return;
            await QrCodeWorker.Instance.Init(p.Name);
            QrCodeWorker.Instance.Serial.ReceiveHandler += Helper_ReceiveHandler;

        }

        private void Helper_ReceiveHandler(byte[] obj)
        {
            var url = Encoding.UTF8.GetString(obj).Trim();
            LogFactory.Create().Info(url);
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
    }
}
