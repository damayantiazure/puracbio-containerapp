using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class CheckConfigurationRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly CheckConfigurationRepository _sut;

    public CheckConfigurationRepositoryTests()
    {
        _sut = new CheckConfigurationRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task
        GetCheckConfigurationsForEnvironmentAsync_WithCorrectParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var environmentId = _fixture.Create<int>();
        var checkConfigurations = _fixture.CreateMany<CheckConfiguration>().ToList();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<CheckConfiguration>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<CheckConfiguration> { Value = checkConfigurations }).Verifiable();

        // Act
        var actual = await _sut.GetCheckConfigurationsForEnvironmentAsync(
            organization, projectId, environmentId, CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(checkConfigurations);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task CreateCheckForEnvironmentAsync_WithCorrectParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var content = _fixture.Create<EnvironmentCheckBodyContent>();
        var checkConfiguration = _fixture.Create<CheckConfiguration>();

        _httpClientCallHandlerMock.Setup(callDistributor =>
                callDistributor.HandlePostCallAsync<CheckConfiguration, EnvironmentCheckBodyContent>(
                    It.IsAny<Uri>(),
                    content, It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(checkConfiguration).Verifiable();

        // Act
        var actual =
            await _sut.CreateCheckForEnvironmentAsync(organization, projectId, content, CancellationToken.None);

        // Assert
        actual.Should().BeEquivalentTo(checkConfiguration);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task DeleteCheckForEnvironmentAsync_WithCorrectParameters_ShouldInvokeCallDistributorMock()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var id = _fixture.Create<string>();

        _httpClientCallHandlerMock.Setup(callDistributor =>
                callDistributor.HandleDeleteCallAsync(
                    It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .Verifiable();

        // Act
        await _sut.DeleteCheckForEnvironmentAsync(organization, projectId, id, CancellationToken.None);

        // Assert
        _httpClientCallHandlerMock.Verify();
    }
}