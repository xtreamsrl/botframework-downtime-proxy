using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Proxy
{
    public interface IUserService
    {
        /// <summary>
        /// It sends a courtesy message to a waiting user. The message can be customized by using the WaitFeedbackUser
        /// env variable, otherwise <see cref="DefaultConfig.WaitFeedbackReply"/> will be used.
        /// 
        /// </summary>
        /// <param name="activity">The user activity that needs a reply</param>
        /// <param name="accessToken">The authentication token to send a message to the user</param>
        /// <returns>An HttpResponseMessage to check if the message has been sent successfully</returns>
        Task<HttpResponseMessage> WaitFeedbackReply(string activity, string accessToken);

        /// <summary>
        /// It get a bot authentication token given bot MicrosoftAppId and MicrosoftAppPassword.
        /// </summary>
        /// <returns>The bot token</returns>
        Task<BotToken> GetBotAuthenticationToken();

        /// <summary>
        /// It redirects the request to the bot.
        /// </summary>
        /// <param name="headers">The request headers</param>
        /// <param name="requestBody">The request body</param>
        /// <returns>An empty task</returns>
        Task BotRedirect(IHeaderDictionary headers, string requestBody);
    }
}