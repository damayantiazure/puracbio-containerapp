using Microsoft.VisualStudio.Services.Common;
using Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;
using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;
using System.Net;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.DelegatingHandlers;

public class AuthenticationDelegateHandlerTests
{
    private class DefaultHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }

    private class HandlerWithRateLimits : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Headers.Add("X-RateLimit-Reset", DateTime.Now.AddMinutes(5).ToUnixEpochTime().ToString());
            return Task.FromResult(responseMessage);
        }
    }

    [Fact]
    public async Task SendAsync_WithExistingAuthenticationHeader_BypassesHandler()
    {
        // Arrange
        var rateLimitObserverMock = new Mock<IAzdoRateLimitObserver>();
        var identityProviderMock = new Mock<IIdentityProvider>();
        var httpRequestMessage = new HttpRequestMessage();
        httpRequestMessage.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("basic");

        var delegateHandler = new AuthenticationDelegateHandler(rateLimitObserverMock.Object, identityProviderMock.Object)
        {
            InnerHandler = new DefaultHandler()
        };

        var invoker = new HttpMessageInvoker(delegateHandler);

        // Act
        await invoker.SendAsync(httpRequestMessage, new CancellationToken());

        // Assert
        rateLimitObserverMock.Verify(m => m.GetAvailableIdentity(), Times.Never);
    }

    [Fact]
    public async Task SendAsync_RateLimitReturned_RateLimitSetOnCredential()
    {
        // Arrange
        const string identity = "id1";
        var rateLimitObserverMock = new Mock<IAzdoRateLimitObserver>();
        var identityProviderMock = new Mock<IIdentityProvider>();
        var authenticationHeaderContextMock = new Mock<IAuthenticationHeaderContext>();
        var httpRequestMessage = new HttpRequestMessage();

        var delegateHandler = new AuthenticationDelegateHandler(rateLimitObserverMock.Object, identityProviderMock.Object)
        {
            InnerHandler = new HandlerWithRateLimits()
        };

        authenticationHeaderContextMock.Setup(m => m.GetAuthenticationHeaderAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new System.Net.Http.Headers.AuthenticationHeaderValue("basic", "secret"));

        rateLimitObserverMock.Setup(m => m.GetAvailableIdentity()).Returns(identity);
        identityProviderMock.Setup(m => m.GetIdentityContext(identity)).Returns(authenticationHeaderContextMock.Object);

        var invoker = new HttpMessageInvoker(delegateHandler);

        // Act
        await invoker.SendAsync(httpRequestMessage, new CancellationToken());

        // Assert
        rateLimitObserverMock.Verify(m => m.GetAvailableIdentity(), Times.Once);
        identityProviderMock.Verify(m => m.GetIdentityContext(identity), Times.Once);
        rateLimitObserverMock.Verify(m => m.RemoveRateLimitDelayForIdentity(identity), Times.Never);
        rateLimitObserverMock.Verify(m => m.SetRateLimitDelayForIdentity(identity, It.IsAny<DateTime>()), Times.Once);
    }
}