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
            var url = new Uri("ws://localhost:8000/v1/covid-19/ws");

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

        public static bool IsActive()
        {
            return client.IsRunning;
        }

        private void HandleMessage(WebsocketClient client, ResponseMessage message)
        {
            LoggerService.Information("WebSocket", "Message received");
            string content = message.Text;
            Console.WriteLine(content);
            WebSocketMessage wsm = JsonConvert.DeserializeObject<WebSocketMessage>(content);
            switch (wsm.code)
            {
                case WebSocketMessageCode.Covid:
                    OnCovidMessageReceived(wsm.data.ToString());
                    break;
            }
        }

        private class WebSocketMessage
        {
            public WebSocketMessageCode code;
            public dynamic data;
        }

        private enum WebSocketMessageCode
        {
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
        public event Action<COVID> CovidMessageReceived;
    }
}