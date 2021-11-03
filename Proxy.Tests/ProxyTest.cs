using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Proxy.Tests
{
    public class ProxyTest
    {
        [Fact]
        public async void ProxyHttpTrigger_TaskCanceledOnBotRedirect_ShouldLogProperly()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(u => u.BotRedirect(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .ThrowsAsync(new TaskCanceledException());
            mockUserService.Setup(u => u.GetBotAuthenticationToken())
                .ReturnsAsync(new BotToken { AccessToken = "accessToken" });
            mockUserService.Setup(u => u.WaitFeedbackReply(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var request = new DefaultHttpContext().Request;
            var proxy = new Proxy(mockUserService.Object);

            await proxy.RunAsync(request, logger);
            string msgBotTimeout = logger.Logs[0];
            string msgUserFeedback = logger.Logs[1];
            Assert.Contains("Timeout has occured during request proxying", msgBotTimeout);
            Assert.Contains("WaitFeedbackReply has been sent with status", msgUserFeedback);
        }

        [Fact]
        public async void ProxyHttpTrigger_HttpRequestExceptionOnBotRedirect_ShouldLogProperly()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(u => u.BotRedirect(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .ThrowsAsync(new TaskCanceledException());
            mockUserService.Setup(u => u.GetBotAuthenticationToken())
                .ReturnsAsync(new BotToken { AccessToken = "accessToken" });
            mockUserService.Setup(u => u.WaitFeedbackReply(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var request = new DefaultHttpContext().Request;
            var proxy = new Proxy(mockUserService.Object);

            await proxy.RunAsync(request, logger);
            string msgBotTimeout = logger.Logs[0];
            string msgUserFeedback = logger.Logs[1];
            Assert.Contains("Timeout has occured during request proxying", msgBotTimeout);
            Assert.Contains("WaitFeedbackReply has been sent with status", msgUserFeedback);
        }

        [Fact]
        public async void ProxyHttpTrigger_ExceptionOnWaitFeedbackReply_ShouldLogProperly()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(u => u.BotRedirect(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .ThrowsAsync(new TaskCanceledException());
            mockUserService.Setup(u => u.GetBotAuthenticationToken())
                .ThrowsAsync(new TaskCanceledException());
            mockUserService.Setup(u => u.WaitFeedbackReply(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var request = new DefaultHttpContext().Request;
            var proxy = new Proxy(mockUserService.Object);

            await proxy.RunAsync(request, logger);
            string msgBotTimeout = logger.Logs[0];
            string msgUserFeedbackException = logger.Logs[1];
            Assert.Contains("Timeout has occured during request proxying", msgBotTimeout);
            Assert.Contains("WaitFeedbackReply has encountered an exception", msgUserFeedbackException);
        }

        [Fact]
        public async void ProxyHttpTrigger_UnexpectedExceptionOnBotRedirect_ShouldLogProperly()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(u => u.BotRedirect(It.IsAny<IHeaderDictionary>(), It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var request = new DefaultHttpContext().Request;
            var proxy = new Proxy(mockUserService.Object);

            await proxy.RunAsync(request, logger);

            string msg = logger.Logs[0];
            Assert.Contains("Unexpected exception has been thrown", msg);
        }

        [Fact]
        public async void ProxyHttpTrigger_SuccessfulBotRedirect_NoLogs()
        {
            var mockUserService = new Mock<IUserService>();
            mockUserService.Setup(u => u.GetBotAuthenticationToken())
                .ReturnsAsync(new BotToken { AccessToken = "accessToken" });
            mockUserService.Setup(u => u.WaitFeedbackReply(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            var request = new DefaultHttpContext().Request;
            var proxy = new Proxy(mockUserService.Object);

            await proxy.RunAsync(request, logger);
            
            Assert.Empty(logger.Logs);
        }
    }
}