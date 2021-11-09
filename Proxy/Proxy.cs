using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Proxy
{
    public class Proxy
    {
        private readonly IUserService _userService;

        public Proxy(IUserService userService)
        {
            _userService = userService;
        }

        [FunctionName("Proxy")]
        public async Task<HttpResponseMessage> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "messages")]
            HttpRequest req, ILogger log)
        {
            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            Task<BotToken> botTokenTask = null;
            try
            {
                botTokenTask = _userService.GetBotAuthenticationToken();
                await _userService.BotRedirect(req.Headers, requestBody);
            }
            catch (Exception e)
                when (e is TaskCanceledException || e is HttpRequestException)
            {
                // HttpRequestException is thrown when the target machine refuse the connection (when it is down for example)
                log.LogDebug("Timeout has occured during request proxying: {ExMessage}", e.Message);
                try
                {
                    var botToken = await botTokenTask!;
                    var waitFeedbackReply = await _userService.WaitFeedbackReply(requestBody, botToken.AccessToken);
                    log.LogDebug("WaitFeedbackReply has been sent with status {StatusCode}",
                        waitFeedbackReply.StatusCode);
                }
                catch (Exception ex)
                {
                    log.LogError("WaitFeedbackReply has encountered an exception: {ExMessage}", ex.Message);
                }
            }
            catch (Exception e)
            {
                log.LogError("Unexpected exception has been thrown: {ExMessage}", e.Message);
            }

            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}