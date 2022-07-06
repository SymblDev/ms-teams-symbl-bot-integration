// <copyright file="JoinCallController.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using PsiBot.Model.Constants;
using PsiBot.Model.Models;
using PsiBot.Service.Settings;
using PsiBot.Services.Bot;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Communications.Common.Telemetry;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using SymblAISharp.Conversation;
using Microsoft.AspNetCore.Http;
using System.Runtime.Caching;

namespace PsiBot.Services.Controllers
{
    /// <summary>
    /// JoinCallController is a third-party controller (non-Bot Framework) that can be called in CVI scenario to trigger the bot to join a call.
    /// </summary>
    public class JoinCallController : ControllerBase
    {
        /// <summary>
        /// The logger
        /// </summary>
        private readonly IGraphLogger _logger;

        /// <summary>
        /// The bot service
        /// </summary>
        private readonly IBotService _botService;

        /// <summary>
        /// The bot configuration
        /// </summary>
        private readonly BotConfiguration botConfiguration;

        ObjectCache cache = MemoryCache.Default;
        private string ConversationIdCacheKey = "ConversationIdCacheKey";

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinCallController" /> class.

        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="eventPublisher">The event publisher.</param>
        /// <param name="botService">The bot service.</param>
        /// <param name="settings">The bot configuration.</param>
        public JoinCallController(IBotService botService, IOptions<BotConfiguration> botConfiguration, IGraphLogger logger)
        {
            _logger = logger;
            _botService = botService;
            this.botConfiguration = botConfiguration.Value;
        }


        [HttpGet]
        [Route(HttpRouteConstants.ConversationAnalytics)]
        public IActionResult ConversationAnalytics(string callLegId)
        {
            if(!_botService.CallHandlers.ContainsKey(callLegId))
                return NotFound();

            var callHandler = _botService.CallHandlers[callLegId];
            string conversationId = (string)cache.Get(string.Format("{0}-{1}", ConversationIdCacheKey, callHandler.Call.Id));

            if (conversationId == null)
                return NotFound();

            SymblAuth symblAuth = new SymblAuth();
            var authResponse = symblAuth.GetAuthToken();
            if(authResponse != null && conversationId != null)
            {
                IConversationApi conversationApi = new ConversationApi(authResponse.accessToken);
                var analyticsResponse = conversationApi.GetAnalytics(conversationId);
                if(analyticsResponse != null)
                {
                    return Ok(analyticsResponse);
                }
                else
                {
                    return NotFound();
                }
            }
            return StatusCode(StatusCodes.Status500InternalServerError);
        }


        [HttpGet]
        [Route(HttpRouteConstants.SubscribeInfo)]
        public IActionResult SubscribeInfo(string callLegId)
        {
            try
            {
                var callHandler = _botService.CallHandlers[callLegId];
                if (callHandler != null)
                {
                    var botMediaStream = callHandler.BotMediaStream;
                    if (botMediaStream != null)
                    {
                        var webSocketService = botMediaStream.GetSymblWebSocketService();
                        if (webSocketService != null)
                        {
                            string accessToken = "";

                            var sybmlAuthResponse = webSocketService.GetSybmlAccessToken();
                            if (sybmlAuthResponse != null)
                            {
                                accessToken = sybmlAuthResponse.accessToken;
                            }

                            string meetingId = webSocketService.GetUniqueMeetingId();
                            string symblSubscribeApiEndpoint = $"wss://api.symbl.ai/v1/subscribe/{meetingId}?access_token={accessToken}";

                            var subscribeApiResponse = new SubscribeApiResponse
                            {
                                AccessToken = accessToken,
                                ConnectionId = meetingId,
                                Url = symblSubscribeApiEndpoint
                            };

                            var json = JsonConvert.SerializeObject(subscribeApiResponse);
                            return Ok(json);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Received HTTP {this.Request.Method}, {this.Request.Path.Value}");
                return StatusCode(500, e.Message);
            }

            return StatusCode(404, "Unable to build the subscribe api link");
        }

        /// <summary>
        /// The join call async.
        /// </summary>
        /// <param name="joinCallBody">The join call body.</param>
        /// <returns>The <see cref="HttpResponseMessage" />.</returns>
        [HttpPost]
        [Route(HttpRouteConstants.JoinCall)]
        public async Task<IActionResult> JoinCallAsync([FromBody] JoinCallBody joinCallBody)
        {
            try
            {
                var call = await _botService.JoinCallAsync(joinCallBody).ConfigureAwait(false);
                var callPath = $"/{HttpRouteConstants.CallRoute.Replace("{callLegId}", call.Id)}";
                var callUri = $"{botConfiguration.ServiceCname}{callPath}";

                var values = new JoinURLResponse()
                {
                    Call = callUri,
                    CallId = call.Id,
                    ScenarioId = call.ScenarioId,
                    Logs = callUri.Replace("/calls/", "/logs/")
                };

                var json = JsonConvert.SerializeObject(values);

                return Ok(values);
            }
            catch (ServiceException e)
            {
                HttpResponseMessage response = (int)e.StatusCode >= 300
                    ? new HttpResponseMessage(e.StatusCode)
                    : new HttpResponseMessage(HttpStatusCode.InternalServerError);

                if (e.ResponseHeaders != null)
                {
                    foreach (var responseHeader in e.ResponseHeaders)
                    {
                        response.Headers.TryAddWithoutValidation(responseHeader.Key, responseHeader.Value);
                    }
                }

                response.Content = new StringContent(e.ToString());
                return StatusCode(500, e.ToString());
            }
            catch (Exception e)
            {
                _logger.Error(e, $"Received HTTP {this.Request.Method}, {this.Request.Path.Value}");
                return StatusCode(500, e.Message);
            }
        }
    }
}
