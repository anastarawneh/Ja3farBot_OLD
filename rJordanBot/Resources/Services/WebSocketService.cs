using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using rJordanBot.Resources.Datatypes;
using Websocket.Client;

namespace rJordanBot.Resources.Services
{
    public class WebSocketService
    {
        static ManualResetEvent resetEvent = new ManualResetEvent(false);
        static WebsocketClient client;
        public void Initialize()
        {
            var exitEvent = new ManualResetEvent(false);
            var url = new Uri("wss://api.anastarawneh.tech/v1/covid-19/ws");

            Task.Run(() => {
                try {
                    using (client = new WebsocketClient(url))
                    {
                        client.ReconnectTimeout = null;
                        client.ErrorReconnectTimeout = null;
                        client.ReconnectionHappened.Subscribe(info =>
                            LoggerService.Information("WebSocket", $"Reconnection happened, type: {info.Type}"));
                        client.DisconnectionHappened.Subscribe(info =>
                            {
                                LoggerService.Information("WebSocket", $"Disconnection happened, type: {info.Type}");
                                LoggerService.Information("WebSocket", info.Exception.ToString());
                            });

                        client.MessageReceived.Subscribe((msg) => HandleMessage(client, msg));
                        client.Start();
                        LoggerService.Information("WebSocket", "WebSocket client started");

                        resetEvent.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    LoggerService.Exception(ex);
                }
            });
        }

        public bool IsActive()
        {
            return client.IsRunning;
        }

        public void Disconnect()
        {
            client.Stop(WebSocketCloseStatus.NormalClosure, "Manual disconnection");
        }

        public void SimulateMessage(string data)
        {
            ResponseMessage msg = ResponseMessage.TextMessage(data);
            client.StreamFakeMessage(msg);
        }

        private void HandleMessage(WebsocketClient client, ResponseMessage message)
        {
            LoggerService.Information("WebSocket", "Message received");
            string content = message.Text;
            LoggerService.Debug("WebSocket", content);
            try
            {
                WebSocketMessage wsm = JsonConvert.DeserializeObject<WebSocketMessage>(content);
                switch (wsm.code)
                {
                    case WebSocketMessageCode.Covid:
                        OnCovidMessageReceived(wsm.data.ToString());
                        break;
                    case WebSocketMessageCode.Test:
                        OnTestMessageReceived(wsm.data.ToString());
                        break;
                }
            }
            catch
            {
                LoggerService.Warning("WebSocket", "Received a message in the wrong format. Ignoring.");
            }
        }

        private class WebSocketMessage
        {
            public WebSocketMessageCode code;
            public dynamic data;
        }

        private enum WebSocketMessageCode
        {
            Test = -1,
            Any = 0,
            Covid = 1
        }

        protected virtual void OnCovidMessageReceived(string data)
        {
            COVID stats = JsonConvert.DeserializeObject<COVID>(data);
            Action<COVID> handler = CovidMessageReceived;
            if (handler != null)
            {
                handler(stats);
            }
        }
        protected virtual void OnTestMessageReceived(string data)
        {
            Action<string> handler = TestMessageReceived;
            if (handler != null)
            {
                handler(data);
            }
        }

        public event Action<COVID> CovidMessageReceived;
        public event Action<string> TestMessageReceived;
    }
}