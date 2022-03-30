// <copyright file="JoinURLResponse.cs" company="Microsoft Corporation">
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT license.
// </copyright>

using Newtonsoft.Json;

namespace PsiBot.Model.Models
{
    public class SubscribeApiResponse
    {
        [JsonProperty("accessToken")]
        public string AccessToken { get; set; }

        [JsonProperty("connectionId")]
        public string ConnectionId { get; set; }
        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
