using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class ApplicationGroupRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly ApplicationGroupRepository _sut;

    public ApplicationGroupRepositoryTests()
    {
        _sut = new ApplicationGroupRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetApplicationGroupForRepositoryAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var applicationGroup = _fixture.Create<ApplicationGroup?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ApplicationGroup>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationGroup).Verifiable();

        // Act
        var actual = await _sut.GetApplicationGroupForRepositoryAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(applicationGroup);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetApplicationGroupsForGroupAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var applicationGroups = _fixture.CreateMany<ApplicationGroup>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<ApplicationGroup>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<ApplicationGroup> { Value = applicationGroups }).Verifiable();

        // Act
        var actual = await _sut.GetApplicationGroupsForGroupAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(applicationGroups);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetApplicationGroupsForProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var applicationGroups = _fixture.CreateMany<ApplicationGroup>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<ApplicationGroup>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<ApplicationGroup> { Value = applicationGroups }).Verifiable();

        // Act
        var actual = await _sut.GetApplicationGroupsForProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(applicationGroups);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetApplicationGroupsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var applicationGroups = _fixture.CreateMany<ApplicationGroup>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<ApplicationGroup>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResponseCollection<ApplicationGroup> { Value = applicationGroups }).Verifiable();

        // Act
        var actual = await _sut.GetApplicationGroupsAsync(_fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(applicationGroups);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetScopedApplicationGroupForProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var applicationGroup = _fixture.Create<ApplicationGroup?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ApplicationGroup>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationGroup).Verifiable();

        // Act
        var actual = await _sut.GetScopedApplicationGroupForProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(applicationGroup);
        _httpClientCallHandlerMock.Verify();
    }
}