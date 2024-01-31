using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class ReleaseRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IVsrmHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly ReleaseRepository _sut;

    public ReleaseRepositoryTests() =>
        _sut = new ReleaseRepository(_httpClientCallHandlerMock.Object);

    [Fact]
    public async Task GetReleaseDefinitionByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var releaseDefinition = new ReleaseDefinition();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ReleaseDefinition>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinition).Verifiable();

        // Act
        var actual = await _sut.GetReleaseDefinitionByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(releaseDefinition);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseDefinitionRevisionByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<string>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<string>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response).Verifiable();

        // Act
        var actual = await _sut.GetReleaseDefinitionRevisionByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<int>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseDefinitionsByProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var releaseDefinition1 = new ReleaseDefinition { Id = _fixture.Create<int>() };
        var releaseDefinition2 = new ReleaseDefinition { Id = _fixture.Create<int>() };
        var releaseDefinitions = new ResponseCollection<ReleaseDefinition> { Count = 2, Value = new[] { releaseDefinition1, releaseDefinition2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<ReleaseDefinition>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitions).Verifiable();

        // Act
        var actual = await _sut.GetReleaseDefinitionsByProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(releaseDefinition1.Id));
        actual.Should().Contain(x => x.Id.Equals(releaseDefinition2.Id));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseApprovalsByReleaseIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var approvals = _fixture.Build<ReleaseApproval>()
            .Without(f => f.Approver)
            .Without(f => f.ApprovedBy)
            .Without(f => f.History)
            .CreateMany()
            .ToList();
        var response = new ResponseCollection<ReleaseApproval> { Count = approvals.Count, Value = approvals };

        _httpClientCallHandlerMock.Setup(m => m.HandleGetCallAsync<ResponseCollection<ReleaseApproval>>(
                It.IsAny<Uri>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var actual = await _sut.GetReleaseApprovalsByReleaseIdAsync(
            _fixture.Create<string>(),
            _fixture.Create<Guid>(),
            _fixture.Create<int>(),
            _fixture.Create<ApprovalStatus>(),
            _fixture.Create<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response.Value);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseSettingsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<ReleaseSettings>();

        _httpClientCallHandlerMock.Setup(m => m.HandleGetCallAsync<ReleaseSettings>(
                It.IsAny<Uri>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var actual = await _sut.GetReleaseSettingsAsync(
            _fixture.Create<string>(),
            _fixture.Create<Guid>(),
            _fixture.Create<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseTagsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var tags = _fixture.CreateMany<string>().ToList();
        var response = new ResponseCollection<string> { Count = tags.Count, Value = tags };

        _httpClientCallHandlerMock.Setup(m => m.HandleGetCallAsync<ResponseCollection<string>>(
                It.IsAny<Uri>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var actual = await _sut.GetReleaseTagsAsync(
            _fixture.Create<string>(),
            _fixture.Create<Guid>(),
            _fixture.Create<int>(),
            _fixture.Create<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response.Value);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseTaskLogByTaskIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var response = _fixture.Create<string>();

        _httpClientCallHandlerMock.Setup(m => m.HandleGetCallAsync<string>(
                It.IsAny<Uri>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(response)
            .Verifiable();

        // Act
        var actual = await _sut.GetReleaseTaskLogByTaskIdAsync(
            _fixture.Create<string>(),
            _fixture.Create<Guid>(),
            _fixture.Create<int>(),
            _fixture.Create<int>(),
            _fixture.Create<int>(),
            _fixture.Create<int>(),
            _fixture.Create<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(response);
        _httpClientCallHandlerMock.Verify();
    }
}