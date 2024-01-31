using Microsoft.VisualStudio.Services.Location;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class AuthorizationRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly AuthorizationRepository _sut;

    public AuthorizationRepositoryTests()
    {
        _sut = new AuthorizationRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetUserForAccessToken_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var connectionData = new ConnectionData();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ConnectionData>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(connectionData).Verifiable();

        // Act
        var actual = await _sut.GetUserForAccessToken(_fixture.Create<AuthenticationHeaderValue>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(connectionData);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetUserForAccessToken_WithInCorrectlyFilledParameters_ShouldNotBeEquivalentToExpectedResponse()
    {
        // Arrange
        var connectionData = new ConnectionData { InstanceId = Guid.NewGuid() };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ConnectionData>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null).Verifiable();

        // Act
        var actual = await _sut.GetUserForAccessToken(_fixture.Create<AuthenticationHeaderValue>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeEquivalentTo(connectionData);
        _httpClientCallHandlerMock.Verify();
    }
}