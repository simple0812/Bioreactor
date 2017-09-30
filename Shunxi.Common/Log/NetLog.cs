using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using Shunxi.Infrastructure.Common.Configuration;
using SuperSocket.ClientEngine;
using WebSocket4Net;
using WebSocket = WebSocket4Net.WebSocket;
using WebSocketState = WebSocket4Net.WebSocketState;

namespace Shunxi.Common.Log
{
    public class NetLog : ILog
    {
        byte[] bytes = new byte[1024];
        private readonly WebSocket websocket;
        private readonly string _localIp;

        private readonly ConcurrentQueue<string> _waitForSendMsg = new ConcurrentQueue<string>();

        public NetLog()
        {
            _localIp = Utility.Common.GetLocalIpex();
            websocket = new WebSocket($"ws://{Config.SERVER_ADDR}:{Config.LOG_PORT}/");
            websocket.Opened += websocket_Opened;
            websocket.Error += Websocket_Error;
            websocket.Closed += Websocket_Closed;
            websocket.MessageReceived += Websocket_MessageReceived;
            websocket.Open();
            Init();
        }

        private void Init()
        {
            Task.Run(async () =>
            {
                string msg = string.Empty;
                while (true)
                {
                    if (websocket.State == WebSocketState.Open && _waitForSendMsg.TryDequeue(out msg))
                    {
                        try
                        {
                            websocket.Send(msg);
                        }
                        catch (Exception e)
                        {
                            Info("send log err " + e.Message);
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
                            Info("open log err "  + e.Message);
                        }
                    }

                    await Task.Delay(10);
                }
            });
        }

        public void Info(string msg)
        {
            Send(msg, LogLevel.Info);
        }

        public void Warnning(string msg)
        {
            Send(msg, LogLevel.Warning);
        }

        public void Error(string msg)
        {
            Send(msg, LogLevel.Error);
        }

        private void open()
        {
            websocket.Open();
        }
        private void websocket_Opened(object sender, EventArgs e)
        {
            Info("log ws open success");
        }

        private void Websocket_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
        }

        private void Websocket_Closed(object sender, EventArgs e)
        {
            Info("log ws close");
        }

        private void Websocket_Error(object sender, ErrorEventArgs ex)
        {
            Info("log ws error " + ex.Exception.Message);
        }

        private void Send(string msg, LogLevel level)
        {
            var now = DateTime.Now;
            var msgx = $"[{_localIp}|{now:yyyy-MM-dd HH:mm:ss.fff} {level}] {msg}";
            Debug.WriteLine(msgx);

            if(_waitForSendMsg.Count <= 100)
                _waitForSendMsg.Enqueue(msgx);
        }
    }
}
