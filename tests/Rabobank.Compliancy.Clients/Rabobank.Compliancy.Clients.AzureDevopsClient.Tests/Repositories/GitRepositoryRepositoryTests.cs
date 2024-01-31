using Microsoft.TeamFoundation.SourceControl.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class GitRepositoryRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly GitRepositoryRepository _sut;

    public GitRepositoryRepositoryTests()
    {
        _sut = new GitRepositoryRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task GetBuildDefinitionByIdAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var gitRepository = _fixture.Create<GitRepository>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<GitRepository>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitRepository).Verifiable();

        // Act
        var actual = await _sut.GetGitRepoByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(gitRepository);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var gitRepository = _fixture.Create<GitRepository>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<GitRepository>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitRepository);

        // Act
        var actual = await _sut.GetGitRepoByNameAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(gitRepository);
    }

    [Fact]
    public async Task GetGitReposByProjectAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var gitRepository1 = _fixture.Create<GitRepository>();
        var gitRepository2 = _fixture.Create<GitRepository>();
        var releaseDefinitions = new ResponseCollection<GitRepository> { Count = 2, Value = new[] { gitRepository1, gitRepository2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<GitRepository>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitions).Verifiable();

        // Act
        var actual = await _sut.GetGitReposByProjectAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.Id.Equals(gitRepository1.Id));
        actual.Should().Contain(x => x.Id.Equals(gitRepository2.Id));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetAllPullRequestsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var gitPullRequest1 = new GitPullRequest { ArtifactId = "artifact 1" };
        var gitPullRequest2 = new GitPullRequest { ArtifactId = "artifact 2" };
        var releaseDefinitions = new ResponseCollection<GitPullRequest> { Count = 2, Value = new[] { gitPullRequest1, gitPullRequest2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<GitPullRequest>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitions).Verifiable();

        // Act
        var actual = await _sut.GetAllPullRequestsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.ArtifactId.Equals(gitPullRequest1.ArtifactId));
        actual.Should().Contain(x => x.ArtifactId.Equals(gitPullRequest2.ArtifactId));
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetPullRequestAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var gitPullRequest = new GitPullRequest { ArtifactId = "PR1" };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<GitPullRequest>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitPullRequest);

        // Act
        var actual = await _sut.GetPullRequestAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(gitPullRequest);
    }

    [Fact]
    public async Task GetPullRequestReviewersAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var reviewer1 = new IdentityRefWithVote { UniqueName = "Reviewer 1" };
        var reviewer2 = new IdentityRefWithVote { UniqueName = "Reviewer 2" };
        var releaseDefinitions = new ResponseCollection<IdentityRefWithVote> { Count = 2, Value = new[] { reviewer1, reviewer2 } };

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ResponseCollection<IdentityRefWithVote>>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitions).Verifiable();

        // Act
        var actual = await _sut.GetPullRequestReviewersAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<int>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().NotBeNull();
        actual.Should().Contain(x => x.UniqueName.Equals(reviewer1.UniqueName));
        actual.Should().Contain(x => x.UniqueName.Equals(reviewer2.UniqueName));
        _httpClientCallHandlerMock.Verify();
    }
}