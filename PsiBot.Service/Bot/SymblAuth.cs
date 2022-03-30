using SymblAISharp.Authentication;
using Microsoft.Extensions.Configuration;

namespace PsiBot.Services.Bot
{
    public class SymblAuth
    {
        protected IConfigurationRoot configurationRoot;

        public SymblAuth()
        {
            configurationRoot = new ConfigurationBuilder()
                .AddJsonFile("appSettings.json").Build();
        }

        public AuthResponse GetAuthToken()
        {
            string appId = configurationRoot["BotConfiguration:SybmlAppId"];
            string appSecret = configurationRoot["BotConfiguration:SybmlAppSecret"];

            AuthenticationApi authentication = new AuthenticationApi();

            var authResponse = authentication.GetAuthToken(
                new AuthRequest
                {
                    type = "application",
                    appId = appId,
                    appSecret = appSecret
                });

            return authResponse;
        }
    }
}
