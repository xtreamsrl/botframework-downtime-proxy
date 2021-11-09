using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

namespace Proxy
{
    public class UserService : IUserService
    {
        private readonly HttpClient _microsoftClient;
        private readonly HttpClient _userClient;
        private readonly HttpClient _botClient;
        private readonly string _messagingEndpoint;
        private readonly string _microsoftAppId;
        private readonly string _microsoftAppPassword;
        private readonly string _waitFeedbackReply;

        public UserService(
            IHttpClientFactory httpClientFactory,
            double timeoutInSeconds,
            string messagingEndpoint,
            string microsoftAppId,
            string microsoftAppPassword,
            string waitFeedbackReply)
        {
            _messagingEndpoint = messagingEndpoint;
            _microsoftAppId = microsoftAppId;
            _microsoftAppPassword = microsoftAppPassword;
            _waitFeedbackReply = waitFeedbackReply;
            _userClient = httpClientFactory.CreateClient();

            _microsoftClient = httpClientFactory.CreateClient();
            _microsoftClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);

            _botClient = httpClientFactory.CreateClient();
            _botClient.Timeout = TimeSpan.FromSeconds(timeoutInSeconds);
        }

        public async Task<HttpResponseMessage> WaitFeedbackReply(string activity, string accessToken)
        {
            _userClient.DefaultRequestHeaders.Authorization = AuthenticationHeaderValue.Parse("Bearer " + accessToken);

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                IgnoreNullValues = true
            };
            var requestActivity = JsonSerializer.Deserialize<Activity>(activity, options);
            var replyActivity = requestActivity.CreateReply(_waitFeedbackReply);

            string baseUrl = replyActivity.ServiceUrl.TrimEnd('/');
            string messageUrl = baseUrl +
                                $"/v3/conversations/{replyActivity.Conversation.Id}/activities/{replyActivity.ReplyToId}";
            return await _userClient.PostAsync(messageUrl,
                new StringContent(JsonSerializer.Serialize(replyActivity, options), Encoding.UTF8, "application/json"));
        }

        // See https://docs.microsoft.com/en-us/azure/bot-service/rest-api/bot-framework-rest-connector-authentication?view=azure-bot-service-4.0
        public async Task<BotToken> GetBotAuthenticationToken()
        {
            var oauthParams = new Dictionary<string, string>
            {
                { "grant_type", "client_credentials" },
                { "client_id", _microsoftAppId },
                { "client_secret", _microsoftAppPassword },
                { "scope", "https://api.botframework.com/.default" }
            };

            var response = await _microsoftClient.PostAsync(
                "https://login.microsoftonline.com/botframework.com/oauth2/v2.0/token",
                new FormUrlEncodedContent(oauthParams));
            string content = response.Content.ReadAsStringAsync().Result;
            return JsonSerializer.Deserialize<BotToken>(content);
        }

        public async Task BotRedirect(IHeaderDictionary headers, string requestBody)
        {
            foreach ((string key, var value) in headers)
            {
                if (key.StartsWith("Authorization"))
                {
                    _botClient.DefaultRequestHeaders.Add(key, value.ToString());
                }
            }

            string contentTypeAndEncoding = headers.ContainsKey("Content-Type")
                ? headers.First(k => k.Key.Equals("Content-Type")).Value.ToString()
                : "application/json; charset=utf-8";
            string[] contentTypeAndEncodingSplit = contentTypeAndEncoding.Split("; charset=");

            string contentType = contentTypeAndEncodingSplit[0];
            var encoding = contentTypeAndEncodingSplit.Length > 1
                ? Encoding.GetEncoding(contentTypeAndEncodingSplit[1])
                : null;

            await _botClient.PostAsync(_messagingEndpoint, new StringContent(requestBody, encoding, contentType));
        }
    }
}