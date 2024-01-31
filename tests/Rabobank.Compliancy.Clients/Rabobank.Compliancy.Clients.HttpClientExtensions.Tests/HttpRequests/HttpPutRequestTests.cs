using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public class HttpPutRequestTests : HttpRequestTestBase
{
    [Fact]
    public async Task ExecuteHttpRequest_CallsDistributePutCall_WithCorrectUriAndValue()
    {
        // Arrange
        var sut = new HttpPutRequestTestImplementation(InvariantUnitTestValue, _httpCallHandler.Object);
        var calculatedUriPath = sut.GetProtectedUrl() + sut.GetProtectedQueryStringAsString();

        // Act
        await sut.ExecuteAsync();

        // Assert
        _httpCallHandler.Verify(h =>
            h.HandlePutCallAsync<object, object>(new Uri(calculatedUriPath, UriKind.Relative),
                InvariantUnitTestValue, It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()));
    }
}