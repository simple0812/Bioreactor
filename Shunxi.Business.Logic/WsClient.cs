using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Shunxi.Business.Enums;
using Shunxi.Business.Logic.Controllers;
using Shunxi.Business.Models.cache;
using Shunxi.Common.Log;
using Shunxi.Infrastructure.Common.Configuration;
using Shunxi.Infrastructure.Common.Extension;

namespace Shunxi.Business.Logic
{
    public class WsClient
    {
//        private MessageWebSocket _messageWebSocket;
//        private DataWriter _messageWriter;
        public static readonly WsClient Instance = new WsClient();
        public readonly ConcurrentQueue<string> MsgQueue = new ConcurrentQueue<string>();
        private DeviceInfo _deviceInfo;

        public event Action<object> SaveScheduleHandler;
        public event Action<string, object> ControlHandler;

        private WsClient()
        {
//            _reconnectTimer = new Timer(obj =>
//            {
//                LogFactory.Create().Info("reconnect");
//                Task.Run(() =>
//                {
//                    Open().IgnorCompletion();
//                });
//
//            }, null, Timeout.Infinite, Timeout.Infinite);
            //            try
            //            {
            //                Task.Run(async () =>
            //                {
            //                    await Open();
            //                    while (true)
            //                    {
            //                        var msg = "";
            //
            //                        if (messageWriter == null || messageWebSocket == null)
            //                        {
            //                            await Task.Delay(10);
            //                            continue;
            //                        }
            //
            //                        msgQueue.TryDequeue(out msg);
            //                        if (string.IsNullOrWhiteSpace(msg))
            //                        {
            //                            await Task.Delay(10);
            //                            continue;
            //                        }
            //
            //                        try
            //                        {
            //                            messageWriter.WriteString(msg);
            //                            await messageWriter.StoreAsync();
            //                        }
            //                        catch (Exception ex)
            //                        {
            //                            msgQueue.Enqueue(msg);
            //                            messageWriter?.DetachStream();
            //                            messageWriter?.DetachBuffer();
            //                            messageWriter?.Dispose();
            //                            messageWriter = null;
            //
            //                            messageWebSocket?.Dispose();
            //                            messageWebSocket = null;
            //
            //                            WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
            //
            //                            switch (status)
            //                            {
            //                                case WebErrorStatus.CannotConnect:
            //                                case WebErrorStatus.NotFound:
            //                                case WebErrorStatus.RequestTimeout:
            //                                    LogFactory.Create().Info("Cannot connect to the server. Please make sure " +
            //                                        "to run the server setup script before running the sample.");
            //                                    break;
            //                                case WebErrorStatus.Unknown:
            //
            //                                    break;
            //                                default:
            //                                    LogFactory.Create().Info("Error: " + status);
            //                                    break;
            //                            }
            //                        }
            //
            //                        await Task.Delay(10);
            //                    }
            //
            //                });
            //
            //            }
            //            catch (Exception)
            //            {
            //                // 
            //            }
        }

//        public async Task Open()
//        {
////            try
////            {
////                if (_messageWebSocket != null || Config.IsLocal) return;
////
////                _messageWebSocket = new MessageWebSocket();
////                _messageWebSocket.Control.MessageType = SocketMessageType.Utf8;
////                _messageWebSocket.MessageReceived += MessageReceived;
////                _messageWebSocket.Closed += MessageWebSocket_Closed;
////
////                Uri uri;
////                var ret = TryGetUri($"ws://{Config.SERVER_ADDR}:{Config.REMOTE_CONTROL_SERVER_PORT}", out uri);
////                if (!ret) return;
////                await _messageWebSocket.ConnectAsync(uri);
////                LogFactory.Create().Info("wsclient open success");
////
////                _reconnectTimer.Change(Timeout.Infinite, Timeout.Infinite);
////                _messageWriter = new DataWriter(_messageWebSocket.OutputStream);
////                _deviceInfo = new DeviceInfo()
////                {
////                    clientId = Infrastructure.Common.Utility.Common.GetUniqueId(),
////                    description = Infrastructure.Common.Utility.Common.GetLocalIp(),
////                    payload = CurrentContext.SysCache?.SystemRealTimeStatus
////                };
////                Send(_deviceInfo, "first_conn");
////            }
////            catch (Exception ex)
////            {
////                _messageWriter?.DetachStream();
////                _messageWriter?.DetachBuffer();
////                _messageWriter?.Dispose();
////                _messageWriter = null;
////
////                _messageWebSocket?.Dispose();
////                _messageWebSocket = null;
////
////                var status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
////
////                switch (status)
////                {
////                    case WebErrorStatus.CannotConnect:
////                    case WebErrorStatus.NotFound:
////                    case WebErrorStatus.RequestTimeout:
////                        LogFactory.Create().Info("Cannot connect to the server. Please make sure to run the server setup script before running the sample.");
////                        break;
////                    case WebErrorStatus.Unknown:
////                        break;
////                    default:
////                        LogFactory.Create().Info("Error: " + status);
////                        break;
////                }
////            }
//        }

//        private void MessageWebSocket_Closed(IWebSocket sender, WebSocketClosedEventArgs args)
//        {
////            LogFactory.Create().Info("ws close");
////            _messageWriter?.DetachStream();
////            _messageWriter?.DetachBuffer();
////            _messageWriter?.Dispose();
////            _messageWriter = null;
////
////            _messageWebSocket?.Dispose();
////            _messageWebSocket = null;
////
////            _reconnectTimer.Change(0, 1000);
//           
//        }


        public void Send(object msg, string action = "")
        {
//            var message = new SocketData(msg, action).PackRaw();
//            if (msgQueue.Count < 100)
//                msgQueue.Enqueue(message);
        }

//        private void MessageReceived(MessageWebSocket sender, MessageWebSocketMessageReceivedEventArgs args)
//        {
//            try
//            {
//                using (DataReader reader = args.GetDataReader())
//                {
//                    reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
//
//                    string read = reader.ReadString(reader.UnconsumedBufferLength);
//                    var ret  = JsonConvert.DeserializeObject<SocketData>(read);
//
//                    if (ret == null) return;
//                    OnReceiveHandler(ret);
//                }
//            }
//            catch (Exception ex)
//            {
//                WebErrorStatus status = WebSocketError.GetStatus(ex.GetBaseException().HResult);
//                LogFactory.Create().Info("MessageReceived ERROR:" + status.ToString());
//
//               if (status == WebErrorStatus.ConnectionAborted)
//                {
//                    _messageWriter?.DetachStream();
//                    _messageWriter?.DetachBuffer();
//                    _messageWriter?.Dispose();
//                    _messageWriter = null;
//
//                    _messageWebSocket?.Dispose();
//                    _messageWebSocket = null;
//                }
//            }
//        }

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
                            clientId = Common.Utility.Common.GetUniqueId(),
                            description = Common.Utility.Common.GetLocalIp(),
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
