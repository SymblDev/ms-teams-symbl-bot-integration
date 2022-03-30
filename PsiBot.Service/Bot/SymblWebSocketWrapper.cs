namespace PsiBot.Services.Bot
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Net.WebSockets;
    using System.Threading;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Newtonsoft.Json;
    using Serilog;
    using System.IO;


    /// <summary>
    /// Reused and Refactored code from https://gist.github.com/xamlmonkey/4737291
    /// </summary>
    public class SymblWebSocketWrapper
    {
        private const int ReceiveChunkSize = 8192;
        private CancellationTokenSource cancellationTokenSource;

        private readonly Uri _uri;
        private readonly System.Net.WebSockets.Managed.ClientWebSocket _webSocketClient;

        private Action<SymblWebSocketWrapper> _onConnected;
        private Action<string, SymblWebSocketWrapper> _onMessage;
        private Action<SymblWebSocketWrapper> _onDisconnected;

        protected SymblWebSocketWrapper(string uri)
        {
            _webSocketClient = new System.Net.WebSockets.Managed.ClientWebSocket();
            _webSocketClient.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static SymblWebSocketWrapper Create(string uri)
        {
            return new SymblWebSocketWrapper(uri);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public SymblWebSocketWrapper Connect()
        {
            cancellationTokenSource = new CancellationTokenSource();
            _webSocketClient.ConnectAsync(_uri, cancellationTokenSource.Token)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            CallOnConnected();
            Task.Factory.StartNew(StartListen, cancellationTokenSource.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
            return this;
        }

        public void Disconnect()
        {
            try
            {
                _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    string.Empty, CancellationToken.None)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                LogDebugInfo("Error in Disconnect: " + ex.ToString(), true);
            }
            
            CallOnDisconnected();
        }

        /// <summary>
        /// Set the Action to call when the connection has been established.
        /// </summary>
        /// <param name="onConnect">The Action to call.</param>
        /// <returns></returns>
        public SymblWebSocketWrapper OnConnect(Action<SymblWebSocketWrapper> onConnect)
        {
            _onConnected = onConnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when the connection has been terminated.
        /// </summary>
        /// <param name="onDisconnect">The Action to call</param>
        /// <returns></returns>
        public SymblWebSocketWrapper OnDisconnect(Action<SymblWebSocketWrapper> onDisconnect)
        {
            _onDisconnected = onDisconnect;
            return this;
        }

        /// <summary>
        /// Set the Action to call when a messages has been received.
        /// </summary>
        /// <param name="onMessage">The Action to call.</param>
        /// <returns></returns>
        public SymblWebSocketWrapper OnMessage(Action<string, SymblWebSocketWrapper> onMessage)
        {
            _onMessage = onMessage;
            return this;
        }

        /// <summary>
        /// Send a message to the WebSocket server.
        /// </summary>
        /// <param name="message">The message to send</param>
        public void SendMessage(string message)
        {
            SendMessageAsync(message)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        /// <summary>
        /// Send Start Request
        /// </summary>
        public void SendStartRequest(Speaker speaker)
        {
            StartRequest startRequest = BuildStartRequest(speaker);
            string startRequestJson = JsonConvert.SerializeObject(startRequest);
            SendMessage(startRequestJson);
        }

        public void StopRequest(string message)
        {
            SendMessage(message);
        }

        private StartRequest BuildStartRequest(Speaker speaker)
        {
            return new StartRequest
            {
                type = "start_request",
                meetingTitle = "Websockets How-to",
                insightTypes = new System.Collections.Generic.List<string>
                {
                    "question", "action_item"
                },
                config = new Config
                {
                    languageCode = "en-US",
                    speechRecognition = new SpeechRecognition
                    {
                        encoding = "LINEAR16",
                        sampleRateHertz = 16000
                    }
                },
                speaker = speaker
            };
        }

        public void SendMessage(byte[] bytes)
        {
            SendMessageAsync(bytes)
                .ConfigureAwait(false)
                .GetAwaiter()
                .GetResult();
        }

        private async Task SendMessageAsync(byte[] bytes)
        {
            try
            {
                if (_webSocketClient.State != WebSocketState.Open)
                {
                    throw new Exception("Connection is not open.");
                }

                await _webSocketClient.SendAsync(new ArraySegment<byte>(bytes, 0, bytes.Length),
                          WebSocketMessageType.Binary, true, CancellationToken.None);
            }
            catch(Exception ex)
            {
                LogDebugInfo("Error in SendMessageAsync bytes: " + ex.ToString(), true);
            }
        }


        private async Task SendMessageAsync(string message)
        {
            try
            {
                if (_webSocketClient.State != WebSocketState.Open)
                {
                    throw new Exception("Connection is not open.");
                }

                var messageBuffer = Encoding.UTF8.GetBytes(message);
                await _webSocketClient.SendAsync(new ArraySegment<byte>(messageBuffer,
                             0, messageBuffer.Length), WebSocketMessageType.Text,
                             true, CancellationToken.None);
            }
            catch (Exception ex)
            {
                LogDebugInfo("Error in SendMessageAsync: " + ex.ToString() , true);
            }
        }

        private async Task StartListen()
        {
            LogDebugInfo("StartListen", false);
            WebSocketReceiveResult result;
            var loopToken = cancellationTokenSource.Token;
            var buffer = new byte[ReceiveChunkSize];
            var message = new List<byte>();

            try
            {
                while (!loopToken.IsCancellationRequested && _webSocketClient.State == WebSocketState.Open)
                {
                    do
                    {
                        result = await _webSocketClient.ReceiveAsync(new ArraySegment<byte>(buffer), loopToken);
                        message.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                    } while (!result.EndOfMessage);

                    if(message.Count > 0)
                    {
                        var stringResult = new StringBuilder();
                        var str = Encoding.UTF8.GetString(message.ToArray(), 0, message.Count);
                        stringResult.Append(str);
                        LogDebugInfo(str);
                        CallOnMessage(stringResult);
                        message.Clear();
                    }
                }
            }
            catch (Exception ex)
            {
                LogDebugInfo("Error in StartListen:" + ex.ToString(), true);
            }
        }

        private void CallOnMessage(StringBuilder stringResult)
        {
            LogDebugInfo("CallOnMessage", false);
            if (_onMessage != null)
                RunInTask(() => _onMessage(stringResult.ToString(), this));
        }

        private void CallOnDisconnected()
        {
            LogDebugInfo("CallOnDisconnected", false);
            if (_onDisconnected != null)
                RunInTask(() => _onDisconnected(this));
        }

        private void CallOnConnected()
        {
            LogDebugInfo("CallOnConnected", false);
            if (_onConnected != null)
                RunInTask(() => _onConnected(this));
        }

        private static void RunInTask(Action action)
        {
            LogDebugInfo("RunInTask", false);
            Task.Factory.StartNew(action);
        }

        private static void LogDebugInfo(string message, bool isError = false)
        {
            try
            {
                if(isError)
                {
                    Log.Error("{Ex}", message);
                    return;
                }

                dynamic jsonObject = JsonConvert.DeserializeObject<dynamic>(message);
                dynamic data = jsonObject;

                if(data.type == "message" && data.message.data != null)
                {
                    Console.WriteLine("Conversation Id: " + data.message.data.conversationId);
                    Log.Information("Conversation Id: "+ data.message.data.conversationId);
                }
                if(data.type == "message_response")
                {
                    foreach(dynamic msg in data.messages)
                    {
                        Console.WriteLine("Transcript (more accurate): "+ msg.payload.content);
                    }
                }
                if(data.type == "topic_response")
                {
                    foreach (dynamic topic in data.topics)
                    {
                        Console.WriteLine("Topic: " + topic.phrases);
                    }
                }
                if(data.type == "insight_response")
                {
                    foreach (dynamic insight in data.insights)
                    {
                        Console.WriteLine("Insight: " + insight.payload.content);
                    }
                }
                if(data.type == "message" && data.message.punctuated != null)
                {
                    Console.WriteLine("Live transcript (less accurate): " + data.message.punctuated.transcript);
                }
            }
            catch(Exception ex)
            {
                Debug.WriteLine(message);
                Console.WriteLine(message);
            }
        }
    }
}
