using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Requests.Authentication.Models;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient.Tests.Repositories;

public class LogAnalyticsRepositoryTests
{
    private readonly LogAnalyticsRepository _sut;

    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IHttpClientCallDistributor<IMicrosoftOnlineHandler>> _authenticationCallDistributorMock = new();
    private readonly Mock<IHttpClientCallDistributor<ILogAnalyticsCallHandler>> _logAnalyticsCallDistributorMock = new();
    private readonly Mock<ILogAnalyticsConfiguration> _logAnalyticsConfigurationMock = new();

    public LogAnalyticsRepositoryTests()
    {
        _sut = new LogAnalyticsRepository(_authenticationCallDistributorMock.Object, _logAnalyticsCallDistributorMock.Object,
            _logAnalyticsConfigurationMock.Object);
    }

    [Fact]
    public async Task GetAuthenticationAsync_WhenAuthenticatedSuccesfully_ShouldReturnAuthentication()
    {
        // Arrange
        var authentication = _fixture.Create<Authentication>();
        var tenantId = _fixture.Create<string>();
        var contentParameters = _fixture.CreateMany<KeyValuePair<string, string>>()
            .ToDictionary(x => x.Key, x => x.Value);
        _logAnalyticsConfigurationMock.Setup(x => x.TenantId).Returns(tenantId);
        _logAnalyticsConfigurationMock.Setup(x => x.ContentParameters).Returns(contentParameters);

        _authenticationCallDistributorMock.Setup(x => x.DistributePostCallAsync<Authentication, FormUrlEncodedContent>(It.IsAny<Uri>(), It.IsAny<FormUrlEncodedContent>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(authentication).Verifiable();

        // Act
        var actual = await _sut.GetAuthenticationAsync();

        // Assert
        actual.Should().BeEquivalentTo(authentication);
    }

    [Fact]
    public async Task GetAuthenticationAsync_WithNoRecordsFound_ShouldReturnNull()
    {
        // Arrange
        var tenantId = _fixture.Create<string>();
        var contentParameters = _fixture.CreateMany<KeyValuePair<string, string>>()
            .ToDictionary(x => x.Key, x => x.Value);
        _logAnalyticsConfigurationMock.Setup(x => x.TenantId).Returns(tenantId);
        _logAnalyticsConfigurationMock.Setup(x => x.ContentParameters).Returns(contentParameters);

        _authenticationCallDistributorMock.Setup(x => x.DistributePostCallAsync<Authentication, FormUrlEncodedContent>(It.IsAny<Uri>(), It.IsAny<FormUrlEncodedContent>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Authentication?)null).Verifiable();

        // Act
        var actual = await _sut.GetAuthenticationAsync();

        // Assert
        actual.Should().BeNull();
    }
}