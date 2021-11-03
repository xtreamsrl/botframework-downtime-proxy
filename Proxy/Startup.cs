using System.Net.Http;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Proxy;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Proxy
{
    public class Startup : FunctionsStartup
    {

        public override void Configure(IFunctionsHostBuilder builder)
        {
            var configuration = builder.GetContext().Configuration;
            double timeoutInSeconds = double.Parse(configuration["TimeoutInSeconds"] ?? DefaultConfig.TimeoutInSeconds);
            string messagingEndpoint = configuration["MessagingEndpoint"];
            string microsoftAppId = configuration["MicrosoftAppId"];
            string microsoftAppPassword = configuration["MicrosoftAppPassword"];
            string waitFeedbackReply = configuration["WaitFeedbackReply"] ?? DefaultConfig.WaitFeedbackReply;

            builder.Services.AddHttpClient();
            builder.Services.AddSingleton<IUserService, UserService>(sp =>
                new UserService(sp.GetRequiredService<IHttpClientFactory>(), 
                    timeoutInSeconds, 
                    messagingEndpoint,
                    microsoftAppId, 
                    microsoftAppPassword,
                    waitFeedbackReply));
        }
    }
}