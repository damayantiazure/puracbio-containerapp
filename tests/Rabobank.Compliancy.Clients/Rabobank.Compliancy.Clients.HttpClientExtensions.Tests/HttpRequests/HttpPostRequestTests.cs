using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public class HttpPostRequestTests : HttpRequestTestBase
{
    [Fact]
    public async Task ExecuteHttpRequest_CallsDistributePostCall_WithCorrectUriAndValue()
    {
        // Arrange
        var sut = new HttpPostRequestTestImplementation(InvariantUnitTestValue, _httpCallHandler.Object);
        var calculatedUriPath = sut.GetProtectedUrl() + sut.GetProtectedQueryStringAsString();

        // Act
        await sut.ExecuteAsync();

        // Assert
        _httpCallHandler.Verify(h =>
            h.HandlePostCallAsync<object, object>(new Uri(calculatedUriPath, UriKind.Relative),
                InvariantUnitTestValue, It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()));
    }
}