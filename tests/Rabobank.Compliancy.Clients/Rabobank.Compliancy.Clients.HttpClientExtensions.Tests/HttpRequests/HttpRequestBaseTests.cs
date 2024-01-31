using FluentAssertions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests.TestRequestImplementations;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public class HttpRequestBaseTests : HttpRequestTestBase
{
    [Fact]
    public async Task Execute_CreatesProperUri_WithNormalImplementation()
    {
        // Arrange
        var sut = new HttpGetRequestTestImplementation(_httpCallHandler.Object);
        var expectedUrl = sut.GetProtectedUrl() + sut.GetProtectedQueryStringAsString();
        Uri? actualUri = null;

        _httpCallHandler
            .Setup(h => h.HandleGetCallAsync<object>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .Callback<Uri, AuthenticationHeaderValue?, CancellationToken>((uri, _, _) => actualUri = uri);

        // Act
        await sut.ExecuteAsync();

        // Assert
        actualUri?.ToString().Should().Be(expectedUrl);
    }

    [Fact]
    public async Task Execute_CreatesProperUri_OnNullQueryString()
    {
        // Arrange
        var sut = new HttpGetRequestTestNoQueryImplementation(_httpCallHandler.Object);
        var expectedUrl = sut.GetProtectedUrl();
        Uri? actualUri = null;

        _httpCallHandler
            .Setup(h => h.HandleGetCallAsync<object>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .Callback<Uri, AuthenticationHeaderValue?, CancellationToken>((uri, _, _) => actualUri = uri);

        // Act
        await sut.ExecuteAsync();

        // Assert
        actualUri?.ToString().Should().Be(expectedUrl);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("testToken")]
    public async Task Execute_CallsDistributeWithCorrectCustomInformation_OnCustomTokenImplementation(string? token)
    {
        // Arrange
        var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer", token);
        var sut = new HttpGetRequestTestImplementation(_httpCallHandler.Object);

        // Act
        await sut.ExecuteAsync(authenticationHeaderValue);

        // Assert
        _httpCallHandler.Verify(h =>
            h.HandleGetCallAsync<object>(It.IsAny<Uri>(), authenticationHeaderValue,
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task Execute_Throws_When_Url_Is_Null()
    {
        // Arrange
        var incorrectRequest = new HttpGetRequestTestIncorrectImplementation(_httpCallHandler.Object);

        // Act + Assert
        var action = () => incorrectRequest.ExecuteAsync();
        await action.Should().ThrowAsync<InvalidOperationException>();
    }
}