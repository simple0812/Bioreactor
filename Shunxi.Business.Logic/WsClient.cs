using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models.cache;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using WebSocket = WebSocket4Net.WebSocket;
using WebSocketState = WebSocket4Net.WebSocketState;

namespace Shunxi.Business.Logic
{
    public class WsClient
    {
        public static readonly WsClient Instance = new WsClient();
        public readonly ConcurrentQueue<string> MsgQueue = new ConcurrentQueue<string>();
        private DeviceInfo _deviceInfo;

        public event Action<object> SaveScheduleHandler;
        public event Action<string, object> ControlHandler;

        byte[] bytes = new byte[1024];
        private readonly WebSocket websocket;

        private WsClient()
        {
            websocket = new WebSocket($"ws://{Config.SERVER_ADDR}:{Config.REMOTE_CONTROL_SERVER_PORT}/");
            websocket.Opened += websocket_Opened;
            websocket.Error += Websocket_Error;
            websocket.Closed += Websocket_Closed;
            websocket.MessageReceived += Websocket_MessageReceived;
            websocket.Open();

            _deviceInfo = new DeviceInfo()
            {
                clientId = Common.Utility.Common.GetMacAddress(),
                description = Common.Utility.Common.GetLocalIpex(),
                payload = CurrentContext.SysCache?.SystemRealTimeStatus
            };
            Send(_deviceInfo, "first_conn");
            Init();
        }

        private void Init()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    if (websocket.State == WebSocketState.Open && MsgQueue.TryDequeue(out string msg))
                    {
                        try
                        {
                            websocket.Send(msg);
                        }
                        catch (Exception e)
                        {
                            LogFactory.Create().Info("send log err " + e.Message);
                        }
                    }
                    else if (websocket.State != WebSocketState.Connecting && websocket.State != WebSocketState.Open)
                    {
                        try
                        {
                            websocket.Open();
                        }
                        catch (Exception e)
                        {
                            LogFactory.Create().Info("open log err " + e.Message);
                        }
                    }

                    await Task.Delay(10);
                }
            });
        }

        private void websocket_Opened(object sender, EventArgs e)
        {
            LogFactory.Create().Info("open success");
        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            try
            {
                var ret = JsonConvert.DeserializeObject<SocketData>(e.Message);
                if (ret == null) return;
                OnReceiveHandler(ret);
            }
            catch (Exception exception)
            {
                LogFactory.Create().Info(exception.Message);
            }
        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            LogFactory.Create().Info("ws close");
        }

        private void Websocket_Error(object sender, ErrorEventArgs ex)
        {
            LogFactory.Create().Info("ws error " + ex.Exception.Message);
        }

        public void Send(object msg, string action = "")
        {
            var message = new SocketData(msg, action).PackRaw();
            if (MsgQueue.Count < 100)
                MsgQueue.Enqueue(message);
        }

        private void OnReceiveHandler(SocketData data)
        {
            switch (data.Action)
            {
                case "requireSyncData":
                    {
                        LogFactory.Create().Info("requireSyncData");
                        ControlCenter.Instance.SyncDeviceDataWithServer();
                        break;
                    }
                case "test":
                    {
                        LogFactory.Create().Info("test->" + data);
                        break;
                    }
                case "requireInit":
                    {
                        LogFactory.Create().Info("requireInit");
                        _deviceInfo = new DeviceInfo()
                        {
                            clientId = Common.Utility.Common.GetMacAddress(),
                            description = Common.Utility.Common.GetLocalIpex(),
                            payload = CurrentContext.SysCache?.SystemRealTimeStatus
                        };
                        Send(_deviceInfo, "first_conn");
                        break;
                    }
                case "command":
                    {
                        LogFactory.Create().Info("command->" + data.Data);
                        var cmd = data.Data.ToString();

                        OnControlHandler(cmd, "");
                        break;
                    }
                case "qr":
                    {
                        OnControlHandler("qr", data.Data);
                        break;
                    }
                //#TODO 远程编辑
                case "saveSchedule":
                    {
                        if (CurrentContext.Status == SysStatusEnum.Completed ||
                            CurrentContext.Status == SysStatusEnum.Unknown ||
                            CurrentContext.Status == SysStatusEnum.Ready ||
                            CurrentContext.Status == SysStatusEnum.Discarded)
                        {
                            SystemIntegration systemIntegration;
                            try
                            {
                                systemIntegration = CultivationService.GetCultivations(JsonConvert.SerializeObject(data.Data));

                            }
                            catch (Exception e)
                            {
                                LogFactory.Create().Warnning(e.Message);
                                Send(new { code = e.Message }, "saveScheduleBack");
                                return;
                            }

                            var validErr = "";
                            var isValid = systemIntegration.Check(out validErr);//PumpValidation.CheckCultivation(cultivation.Pumps.ToList(), out validErr);

                            if (isValid)
                            {
                                ControlCenter.Instance.Dispose();
                                DeviceService.InitData();

                                Send(new { code = "success" }, "saveScheduleBack");
                                OnSaveScheduleHandler(systemIntegration);
                            }
                            else
                            {
                                Send(new { code = validErr }, "saveScheduleBack");
                            }
                        }
                        else
                        {
                            Send(new { code = $"Current Status is {CurrentContext.Status}, can not set schedule" },
                                "saveScheduleBack");
                        }

                        break;
                    }
                default:
                    break;
            }
        }

        private void OnControlHandler(string cmd, object data)
        {
            ControlHandler?.Invoke(cmd, data);
        }

        private void OnSaveScheduleHandler(object obj)
        {
            SaveScheduleHandler?.Invoke(obj);
        }

        private bool TryGetUri(string uriString, out Uri uri)
        {
            uri = null;

            Uri webSocketUri;
            if (!Uri.TryCreate(uriString.Trim(), UriKind.Absolute, out webSocketUri))
            {
                LogFactory.Create().Info("Error: Invalid URI");
                return false;
            }

            if (!String.IsNullOrEmpty(webSocketUri.Fragment))
            {
                LogFactory.Create().Info("Error: URI fragments not supported in WebSocket URIs.");
                return false;
            }

            if ((webSocketUri.Scheme != "ws") && (webSocketUri.Scheme != "wss"))
            {
                LogFactory.Create().Info("Error: WebSockets only support ws:// and wss:// schemes.") ;
                return false;
            }

            uri = webSocketUri;

            return true;
        }

    }

    public class DeviceInfo
    {
        public string clientId { get; set; }
        public string description { get; set; }
        public SystemRealTimeStatus payload { get; set; }

        public DeviceInfo()
        {

        }
    }
}
