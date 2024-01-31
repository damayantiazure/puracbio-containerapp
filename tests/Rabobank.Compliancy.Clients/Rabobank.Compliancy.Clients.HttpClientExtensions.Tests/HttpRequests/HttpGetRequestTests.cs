using Microsoft.Identity.Client;
using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public class HttpGetRequestTests : HttpRequestTestBase
{
    [Fact]
    public async Task ExecuteHttpRequest_CallsDistributeGetCall_WithCorrectUri()
    {
        // Arrange
        var sut = new HttpGetRequestTestImplementation(_httpCallHandler.Object);
        var calculatedUriPath = sut.GetProtectedUrl() + sut.GetProtectedQueryStringAsString();

        // Act
        await sut.ExecuteAsync();

        // Assert
        _httpCallHandler.Verify(h => h.HandleGetCallAsync<object>(new Uri(calculatedUriPath, UriKind.Relative),
            It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()));
    }
}