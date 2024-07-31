using Flurl.Http;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class RepositoryTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public RepositoryTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryRepositoryAsync()
    {
        var definition = (await _client.GetAsync(Repository.Repositories(_config.ProjectName))).ToList();
        definition.ShouldNotBeEmpty();
        definition.ShouldAllBe(e => !string.IsNullOrEmpty(e.Name));
        definition.ShouldAllBe(e => !string.IsNullOrEmpty(e.Id));
        definition.ShouldAllBe(e => !string.IsNullOrEmpty(e.Project.Id));
        definition.ShouldAllBe(e => !string.IsNullOrEmpty(e.Project.Name));
        definition.ShouldAllBe(e => !string.IsNullOrEmpty(e.DefaultBranch));
    }

    [Fact]
    public async Task QueryPushes()
    {
        // Arrange
        var repository = (await _client.GetAsync(Repository.Repositories(_config.ProjectName))).First();

        // Act
        var pushes = (await _client.GetAsync(Repository.Pushes(_config.ProjectName, repository.Id))).ToList();

        // Assert
        pushes.ShouldNotBeEmpty();
        var push = pushes[0];
        push.PushId.ShouldNotBe(0);
        push.Date.ShouldNotBe(default);
    }

    [Fact]
    public async Task GetGitRefs()
    {
        //Arrange
        var repository = (await _client.GetAsync(Repository.Repositories(_config.ProjectName))).First();

        //Act
        var gitRefs = (await _client.GetAsync(Repository.Refs(_config.ProjectName, repository.Id))).ToList();

        //Assert
        gitRefs.ShouldNotBeEmpty();
        var gitRef = gitRefs[0];
        gitRef.Name.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetGitItem()
    {
        var gitItem = await _client.GetAsync(Repository.GitItem(_config.ProjectName,
                _config.RepositoryId, _config.GitItemFilePath)
            .AsJson()).ConfigureAwait(false);

        gitItem.ShouldNotBeNull();
    }

    [Fact]
    public void PushThrowsFor404()
    {
        var repositoryId = Guid.NewGuid().ToString();
        var ex = Should.Throw<FlurlHttpException>( () =>
        {
            _client.GetAsync(Repository.Pushes(_config.ProjectName, repositoryId)).GetAwaiter().GetResult();
        });

        ex.Message.ShouldContain(repositoryId);
    }

    [Fact]
    public async Task QueryPullRequestsAsync()
    {
        //Arrange
        const string status = "completed";
        const int top = 5;

        //Act
        var pullRequests = (await _client.GetAsync(Repository.PullRequests(
            _config.ProjectName, _config.RepositoryId, status, top))).ToList();

        // Assert
        pullRequests.Count.ShouldBeLessThanOrEqualTo(top);
        pullRequests.TrueForAll(p => p.Status == status).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryPullRequestsAsyncWithoutProject()
    {
        //Arrange
        const string status = "completed";
        const int top = 5;

        //Act
        var pullRequests = (await _client.GetAsync(Repository.PullRequests(
            _config.RepositoryId, status, top))).ToList();

        // Assert
        pullRequests.Count.ShouldBeLessThanOrEqualTo(top);
        pullRequests.TrueForAll(p => p.Status == status).ShouldBeTrue();
    }

    [Fact]
    public async Task QueryPullRequest()
    {
        //Arrange
        var pullRequestId = (await _client.GetAsync(Repository.PullRequests(
            _config.ProjectName, _config.RepositoryId, "completed", 1))).First().PullRequestId;

        //Act
        var pullRequest = await _client.GetAsync(Repository.PullRequest(
            _config.ProjectName, _config.RepositoryId, pullRequestId)).ConfigureAwait(false);

        // Assert
        pullRequest.ClosedBy.ShouldNotBeNull();
        pullRequest.LastMergeCommit.CommitId.ShouldNotBeNull();
        pullRequest.Reviewers.ShouldNotBeEmpty();
        pullRequest.Reviewers.First().DisplayName.ShouldNotBeNull();
        pullRequest.Reviewers.First().UniqueName.ShouldNotBeNull();
        pullRequest.Reviewers.First().Vote.ShouldBe(10);
        pullRequest.Reviewers.First().IsContainer.ShouldBe(false);
    }

    [Fact]
    public async Task QueryPullRequestWithoutProject()
    {
        //Arrange
        var pullRequestId = (await _client.GetAsync(Repository.PullRequests(
            _config.RepositoryId, "completed", 1))).First().PullRequestId;

        //Act
        var pullRequest = await _client.GetAsync(Repository.PullRequest(
            _config.RepositoryId, pullRequestId)).ConfigureAwait(false);

        // Assert
        pullRequest.ClosedBy.ShouldNotBeNull();
        pullRequest.LastMergeCommit.CommitId.ShouldNotBeNull();
        pullRequest.Reviewers.ShouldNotBeEmpty();
        pullRequest.Reviewers.First().DisplayName.ShouldNotBeNull();
        pullRequest.Reviewers.First().UniqueName.ShouldNotBeNull();
        pullRequest.Reviewers.First().Vote.ShouldBe(10);
        pullRequest.Reviewers.First().IsContainer.ShouldBe(false);
    }

    [Fact]
    public async Task QueryCommit()
    {
        // Act
        var commit = await _client.GetAsync(Repository.Commit(
            _config.ProjectName, _config.RepositoryId, _config.CommitId));

        // Assert
        commit.ShouldNotBeNull();
        commit.Author.Date.ShouldBeGreaterThan(
            new DateTime(2019, 10, 19, default, default, default, DateTimeKind.Local));
        commit.Author.Email.ShouldNotBeNull();
        commit.Author.Name.ShouldNotBeNull();
        commit.Author.ImageUrl.ShouldNotBeNull();
        commit.Comment.ShouldNotBeNull();
        commit.CommitId.ShouldNotBeNull();
        commit.Committer.Date.ShouldBeGreaterThan(new DateTime(2019, 10, 19, default, default, default,
            DateTimeKind.Local));
        commit.Committer.Email.ShouldNotBeNull();
        commit.Committer.Name.ShouldNotBeNull();
        commit.Committer.ImageUrl.ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryCommits()
    {
        // Act
        var commits = (await _client.GetAsync(Repository.Commits(
            _config.ProjectName, _config.RepositoryId))).ToList();

        // Assert
        commits.ShouldNotBeNull();
        commits.Count.ShouldBeGreaterThan(0);
        commits[0].Author.Date
            .ShouldBeGreaterThan(new DateTime(2019, 10, 19, default, default, default, DateTimeKind.Local));
        commits[0].Author.Email.ShouldNotBeNull();
        commits[0].Author.Name.ShouldNotBeNull();
        commits[0].Author.ImageUrl.ShouldBeNull();
        commits[0].Comment.ShouldNotBeNull();
        commits[0].CommitId.ShouldNotBeNull();
        commits[0].Committer.Date
            .ShouldBeGreaterThan(new DateTime(2019, 10, 19, default, default, default, DateTimeKind.Local));
        commits[0].Committer.Email.ShouldNotBeNull();
        commits[0].Committer.Name.ShouldNotBeNull();
        commits[0].Committer.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task QueryPullRequestCommits()
    {
        // Arrange 
        var pullRequestId = (await _client.GetAsync(Repository.PullRequests(
            _config.ProjectName, _config.RepositoryId, "completed", 1))).First().PullRequestId;

        // Act
        var commits = (await _client.GetAsync(Repository.PullRequestCommits(
            _config.ProjectName, _config.RepositoryId, pullRequestId.ToString()))).ToList();

        // Assert
        commits.ShouldNotBeNull();
        commits.Count.ShouldBeGreaterThan(0);
        commits[0].Author.Date
            .ShouldBeGreaterThan(new DateTime(2019, 10, 19, default, default, default, DateTimeKind.Local));
        commits[0].Author.Email.ShouldNotBeNull();
        commits[0].Author.Name.ShouldNotBeNull();
        commits[0].Author.ImageUrl.ShouldBeNull();
        commits[0].Comment.ShouldNotBeNull();
        commits[0].CommitId.ShouldNotBeNull();
        commits[0].Committer.Date
            .ShouldBeGreaterThan(new DateTime(2019, 10, 19, default, default, default, DateTimeKind.Local));
        commits[0].Committer.Email.ShouldNotBeNull();
        commits[0].Committer.Name.ShouldNotBeNull();
        commits[0].Committer.ImageUrl.ShouldBeNull();
    }

    [Fact]
    public async Task QueryPullRequestReviewers()
    {
        // Arrange 
        var pullRequestId = (await _client.GetAsync(Repository.PullRequests(
            _config.ProjectName, _config.RepositoryId, "completed", 1))).First().PullRequestId;

        // Act
        var reviewers = (await _client.GetAsync(Repository.PullRequestReviewers(
            _config.ProjectName, _config.RepositoryId, pullRequestId.ToString()))).ToList();

        // Assert
        reviewers.ShouldNotBeNull();
        reviewers.Count.ShouldBeGreaterThan(0);
        reviewers[0].Id.ShouldNotBeNull();
        reviewers[0].UniqueName.ShouldNotBeNull();
    }
}