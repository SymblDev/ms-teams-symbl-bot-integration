using Microsoft.Graph.Communications.Common.Telemetry;
using Serilog;
using SymblAISharp.Authentication;
using System;
using System.Diagnostics;

namespace PsiBot.Services.Bot
{
    public interface ISymblWebSocketService
    {
        void SendAudio(byte[] audio);
    }

    /// <summary>
    /// The Sybml WebSocket Client service responsible for establishing the 
    /// Sybml WebSocket Connection and sending the audio bytes for each of the connected participants
    /// </summary>
    public class SymblWebSocketService : ISymblWebSocketService
    {
        private AuthResponse authResponse = null;
        private SymblWebSocketWrapper symblWebSocketWrapper;
        private static IGraphLogger logger;

        private readonly string speakerName;
        private readonly string participantId;
        private readonly string callId;

        public SymblWebSocketService(IGraphLogger graphLogger,
            string speakerName, 
            string participantId,
            string callId)
        {
            logger = graphLogger;
            this.participantId = participantId;
            this.callId = callId;
            this.speakerName = speakerName;
        }

        public string GetUniqueMeetingId()
        {
            return SingletonMeetingGenerator.GetUniqueId(callId);
        }

        public AuthResponse GetSybmlAccessToken()
        {
            if(authResponse == null)
                authResponse = new SymblAuth().GetAuthToken();

            return authResponse;
        }

        public void CreateWebSocket()
        {
            var authResponse = GetSybmlAccessToken();

            if(authResponse != null)
            {
                try
                {
                    Log.Logger.Information($"Access Token: " +
                        $"{authResponse.accessToken}");

                    string uniqueMeetingId = GetUniqueMeetingId();
                    string symblEndpoint = $"wss://api.symbl.ai/v1/realtime/insights/{uniqueMeetingId}?access_token={authResponse.accessToken}";

                    Speaker speaker = new Speaker
                    {
                        name = speakerName,
                        userId = speakerName.Replace(" ", "") + "@email.com"
                    };

                    symblWebSocketWrapper = SymblWebSocketWrapper.Create(symblEndpoint, speaker, callId);
                    symblWebSocketWrapper.Connect();

                    symblWebSocketWrapper.SendStartRequest(speaker);

                    Log.Logger.Information($"Unique Meeting Id: " +
                        $"{uniqueMeetingId}");

                    LogDebugInfo($"Unique Meeting Id: {uniqueMeetingId}");
                    LogDebugInfo($"Websocket Connection Opened " +
                        $"for User Id: {participantId} " +
                        $"and Speaker Name: {speakerName}");
                }
                catch(Exception ex)
                {
                    LogDebugInfo($"Error in CreateWebSocket: " + ex.StackTrace);
                }
            }
        }

        private static void LogDebugInfo(string message)
        {
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        public void SendAudio(byte[] audio)
        {
            try
            {
                symblWebSocketWrapper.SendMessage(audio);
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in SendAudio: " + ex.StackTrace);
            }
        }

        public void Disconnect()
        {
            try
            {
                symblWebSocketWrapper.Disconnect();
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in Disconnect: " + ex.StackTrace);
            }
        }

        public void SendStopRequest()
        {
            try
            {
                symblWebSocketWrapper.StopRequest("{\"type\": \"stop_request\"}");
            }
            catch (Exception ex)
            {
                LogDebugInfo($"Error in SendStopRequest: " + ex.StackTrace);
            }
        }
    }
}
