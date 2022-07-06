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
using System.Runtime.Caching;

namespace PsiBot.Services.Bot
{
    /// <summary>
    /// Reused and Refactored code from https://gist.github.com/xamlmonkey/4737291
    /// </summary>
    public class SymblWebSocketWrapper
    {
        private Speaker speaker;
        private int reConnectMaxCount = 1000;
        private const int SendChunkSize = 1024;
        private const int ReceiveChunkSize = 65536;
        private const string MeetingTitle = "MSTeams Symbl Integration";
        private CancellationTokenSource cancellationTokenSource;
        private readonly string callId;

        private readonly Uri _uri;
        private System.Net.WebSockets.Managed.ClientWebSocket _webSocketClient;

        private Action<SymblWebSocketWrapper> _onConnected;
        private Action<string, SymblWebSocketWrapper> _onMessage;
        private Action<SymblWebSocketWrapper> _onDisconnected;

        private readonly CancellationToken _cancellationToken;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly object reconnectLock = new object();


        ObjectCache cache = MemoryCache.Default;
        bool isConversationIdCached = false;
        private string ConversationIdCacheKey = "ConversationIdCacheKey";

        protected SymblWebSocketWrapper(string uri, Speaker speaker, string callId)
        {
            this.speaker = speaker;
            this.callId = callId; 
            _webSocketClient = new System.Net.WebSockets.Managed.ClientWebSocket();
            _webSocketClient.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="uri">The URI of the WebSocket server.</param>
        /// <returns></returns>
        public static SymblWebSocketWrapper Create(string uri, Speaker speaker, string callId)
        {
            return new SymblWebSocketWrapper(uri, speaker, callId);
        }

        /// <summary>
        /// Connects to the WebSocket server.
        /// </summary>
        /// <returns></returns>
        public SymblWebSocketWrapper Connect()
        {
            try
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
            catch (Exception ex)
            {
                LogDebugInfo("Error in Connect: " + ex.ToString(), true);
            }

            return null;
        }

        public void ReConnect()
        {
            lock (reconnectLock)
            {

                LogDebugInfo("WebSocket State: " + _webSocketClient.State.ToString(), true);
                if (reConnectMaxCount > 0)
                {
                    try
                    {
                        _webSocketClient = new System.Net.WebSockets.Managed.ClientWebSocket();

                        _webSocketClient.ConnectAsync(_uri, cancellationTokenSource.Token)
                            .ConfigureAwait(false)
                            .GetAwaiter()
                            .GetResult();
                         SendStartRequest(speaker);
                    }
                    catch (Exception ex)
                    {
                        LogDebugInfo("Error in ReConnect: " + ex.ToString(), true);
                    }
                }
                reConnectMaxCount = reConnectMaxCount - 1;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (_webSocketClient == null)
                    return;
                _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure,
                    "Closing Websocket", CancellationToken.None)
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
                meetingTitle = MeetingTitle,
                insightTypes = new List<string>
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
                noConnectionTimeout = 120,
                disconnectOnStopRequest = false,
                disconnectOnStopRequestTimeout = 60,
                speaker = speaker
            };
        }

        public void SendMessage(byte[] bytes)
        {
            try
            {
                SendMessageAsync(bytes)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception ex)
            {
                LogDebugInfo("Error in SendMessage: " + ex.ToString(), true);
            }
        }

        private async Task SendMessageAsync(byte[] bytes)
        {
            try
            {
                if (_webSocketClient.State == WebSocketState.Open)
                {
                    int offSet = 0;
                    foreach (var chunkedBytes in Split(bytes, SendChunkSize))
                    {
                        if ((offSet + SendChunkSize) <= bytes.Length)
                        {
                            await _webSocketClient.SendAsync(new ArraySegment<byte>(bytes, offSet, SendChunkSize),
                               WebSocketMessageType.Binary, true, _cancellationToken);
                            offSet += SendChunkSize;
                        }
                    }

                    int lastBytes = bytes.Length - offSet;
                    if (lastBytes > 0)
                    {
                        await _webSocketClient.SendAsync(new ArraySegment<byte>(bytes, offSet, lastBytes),
                               WebSocketMessageType.Binary, true, _cancellationToken);
                    }
                }
                else
                {
                    LogDebugInfo("Websocket Connection State is not Open in Method SendMessageAsync");
                }
            }
            catch(WebSocketException ex)
            {
                ReConnect();
            }
            catch (Exception ex)
            {
                LogDebugInfo("Error in SendMessageAsync: " + ex.ToString(), true);
            }
        }

        /// <summary>
        /// https://stackoverflow.com/questions/11816295/splitting-a-byte-into-multiple-byte-arrays-in-c-sharp
        /// </summary>
        /// <param name="value"></param>
        /// <param name="bufferLength"></param>
        /// <returns></returns>
        public static IEnumerable<byte[]> Split(byte[] value,
            int bufferLength)
        {
            int countOfArray = value.Length / bufferLength;
            if (value.Length % bufferLength > 0)
                countOfArray++;

            for (int i = 0; i < countOfArray; i++)
            {
                yield return value.Skip(i * bufferLength).Take(bufferLength)
                    .ToArray();
            }
        }


        private async Task SendMessageAsync(string message)
        {
            try
            {
                if (_webSocketClient.State == WebSocketState.Open)
                {
                    var messageBuffer = Encoding.UTF8.GetBytes(message);
                    await _webSocketClient.SendAsync(new ArraySegment<byte>(messageBuffer,
                                 0, messageBuffer.Length), WebSocketMessageType.Text,
                                 true, CancellationToken.None);
                }
                else
                {
                    LogDebugInfo("Websocket Connection State is not Open in Method SendMessageAsync");
                }
            }
            catch (WebSocketException ex)
            {
                ReConnect();
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
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await _webSocketClient.CloseAsync(WebSocketCloseStatus.NormalClosure,
                                string.Empty, CancellationToken.None);
                            ReConnect();
                        }
                        else
                        {
                            message.AddRange(new ArraySegment<byte>(buffer, 0, result.Count));
                        }
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
            Task.Factory.StartNew(action);
        }

        private void LogDebugInfo(string message, bool isError = false)
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

                string cachedConversationId = (string)cache.Get(string.Format("{0}-{1}", ConversationIdCacheKey, callId));
                if (!string.IsNullOrEmpty(cachedConversationId))
                    isConversationIdCached = true;

                if(data.type == "message" && data.message.data != null && isConversationIdCached == false)
                {
                    Console.WriteLine("Conversation Id: " + data.message.data.conversationId);
                    Log.Information("Conversation Id: "+ data.message.data.conversationId);

                    cache.Set(string.Format("{0}-{1}", ConversationIdCacheKey, callId), data.message.data.conversationId.ToString(), 
                        DateTimeOffset.Now.Add(TimeSpan.FromHours(24)));
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
