#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using NSubstitute;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Model;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Approvals.Tests.Services;

public class PullRequestApproverServiceTests
{
    private const int _approvedVote = 10;
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public async Task BuildWithoutChangesWithoutGitCommitShouldReturnFalse()
    {
        // Arrange
        var client = Substitute.For<IAzdoRestClient>();
        client
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(new List<Change>());

        var build = _fixture.Create<Build>();
        client
            .GetAsync(Arg.Any<IAzdoRequest<Build>>())
            .Returns(build);

        var logQueryService = Substitute.For<ILogQueryService>();
        var sut = new PullRequestApproverService(client, logQueryService);

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task NoArtifactsAttachedToClassicPipeline()
    {
        // Arrange
        var client = Substitute.For<IAzdoRestClient>();
        client
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(_fixture.CreateMany<Change>(0));

        var build = _fixture.Create<Build>();
        client
            .GetAsync(Arg.Any<IAzdoRequest<Build>>())
            .Returns(build);

        var logQueryService = Substitute.For<ILogQueryService>();

        var sut = new PullRequestApproverService(client, logQueryService);

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task BuildWithoutChangesWithGitCommitShouldReturnFalse()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        var azdoClient = Substitute.For<IAzdoRestClient>();
        azdoClient
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(new List<Change>());

        var build = _fixture.Create<Build>();
        build.Repository.Type = "TfsGit";
        build.SourceVersion = "39b4c8d3acb868a956baeee248c3376fdc307ad3";

        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<Build>>())
            .Returns(build);

        var sut = new PullRequestApproverService(azdoClient, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task BuildWithoutGitTypeShouldNotBeAllowed()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        _fixture.Customize<Change>(x => x
            .With(c => c.Type, "Tfvc")
            .With(c => c.Id, "123"));
        _fixture.Customize<GitCommitRef>(x => x
            .With(c => c.CommitId, "123"));

        var logQueryService = Substitute.For<ILogQueryService>();
        var client = new FixtureClient(_fixture);

        var sut = new PullRequestApproverService(client, logQueryService);

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task CommitWithoutPullRequestShouldNotBeAllowed()
    {
        // Arrange
        const string commitId1 = "123";
        const string commitId2 = "456";

        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        _fixture.Customize<Change>(x => x
            .With(c => c.Id, commitId1)
            .With(c => c.Type, "TfsGit"));
        _fixture.Customize<GitCommitRef>(x => x
            .With(c => c.CommitId, commitId2));

        var client = new FixtureClient(_fixture);
        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task PullRequestApprovedByGroupShouldNotBeAllowed()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        _fixture.Customize<IdentityRefWithVote>(x => x
            .With(i => i.Id, "GroupName")
            .With(i => i.IsContainer, true)
            .With(i => i.Vote, 10));

        var client = new FixtureClient(_fixture);
        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task PullRequestIsNotApprovedShouldNotBeAllowed()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        _fixture.Customize<IdentityRefWithVote>(x => x
            .With(i => i.Id, "Identity")
            .With(i => i.IsContainer, false)
            .With(i => i.Vote, 0));

        var client = new FixtureClient(_fixture);
        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approver = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approver.Any().ShouldBeFalse();
    }

    [Fact]
    public async Task PullRequestCloserIsApproverShouldBeNotAllowedButReturnApprover()
    {
        // Arrange
        const string approver = "jan.jansen@rabobank.com";
        var identityId = Guid.NewGuid();
        SetupFixture(approver, approver);
        _fixture.Customize<IdentityRefWithVote>(x => x
            .With(i => i.Id, identityId.ToString())
            .With(i => i.IsContainer, false)
            .With(i => i.Vote, 10)
            .With(i => i.UniqueName, approver));
        _fixture.Customize<IdentityRef>(x => x
            .With(i => i.Id, identityId));

        var client = new FixtureClient(_fixture);
        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approvers = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(false);
        approvers.Single().ShouldBe(approver);
    }

    [Fact]
    public async Task GetAllApproversAsync_AllWithApprovedVote_ShouldReturnApprovers()
    {
        // Arrange
        const string approver = "jan.jansen@rabobank.com";
        SetupFixture(approver, approver);
        var client = new FixtureClient(_fixture);

        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approvers = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(true);
        approvers.Single().ShouldBe(approver);
    }

    [Fact]
    public async Task GetAllApproversAsync_ApprovedWithSuggestions_ShouldReturnApprovers()
    {
        // Arrange                       
        const int approvedWithSuggestionsVote = 5;
        const string approver = "jan.jansen@rabobank.com";
        SetupFixture(approver, approver, approvedWithSuggestionsVote);
        var client = new FixtureClient(_fixture);

        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var result = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());
        var approvers = await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        result.ShouldBe(true);
        approvers.Single().ShouldBe(approver);
    }

    [Fact]
    public async Task HasApprovalAsync_IncompleteDataInLogAnalytics_UseFallback()
    {
        // Arrange
        const string approver = "jan.jansen@rabobank.com";
        SetupFixture(approver, approver);
        var client = new FixtureClient(_fixture);

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(new List<string>(), "test@test.nl", null, "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var sut = new PullRequestApproverService(client, Substitute.For<ILogQueryService>());

        // Act
        var actual = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        actual.ShouldBe(true);
    }

    [Fact]
    public async Task AllUniqueApproversShouldBeReturned()
    {
        // Arrange
        const string approver = "jan.jansen@rabobank.com";
        SetupFixture(approver, approver);
        var azdoClient = Substitute.For<IAzdoRestClient>();
        azdoClient
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(_fixture.CreateMany<Change>());
        azdoClient
            .GetAsync(Arg.Any<IEnumerableRequest<GitPullRequest>>())
            .Returns(_fixture.CreateMany<GitPullRequest>());
        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<GitPullRequest>>())
            .Returns(_fixture.Create<GitPullRequest>());
        azdoClient
            .GetAsync(Arg.Any<IEnumerableRequest<IdentityRefWithVote>>())
            .Returns(_fixture.CreateMany<IdentityRefWithVote>(3));
        azdoClient
            .GetAsync(Arg.Any<MemberEntitlementManagementRequest<UserEntitlement>>())
            .Returns(_fixture.Create<UserEntitlement>());

        var logAnalyticsClient = Substitute.For<ILogQueryService>();

        var sut = new PullRequestApproverService(azdoClient, logAnalyticsClient);

        // Act
        var approvers = (await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>())).ToList();

        // Assert
        approvers.Count.ShouldBe(1);
        approvers.Single().ShouldBe(approver);
    }

    [Fact]
    public async Task GetAllApproversAsync_IncompleteDataInLogAnalytics_FallBackUsed()
    {
        // Arrange
        const string approver = "jan.jansen@rabobank.com";
        SetupFixture(approver, approver);

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(new List<string>(), "test@test.nl", null, "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var client = Substitute.For<IAzdoRestClient>();
        client
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(_fixture.CreateMany<Change>());
        client
            .GetAsync(Arg.Any<IEnumerableRequest<GitPullRequest>>())
            .Returns(_fixture.CreateMany<GitPullRequest>());
        client
            .GetAsync(Arg.Any<IAzdoRequest<GitPullRequest>>())
            .Returns(_fixture.Create<GitPullRequest>());
        client
            .GetAsync(Arg.Any<IEnumerableRequest<IdentityRefWithVote>>())
            .Returns(_fixture.CreateMany<IdentityRefWithVote>(3));
        client
            .GetAsync(Arg.Any<MemberEntitlementManagementRequest<UserEntitlement>>())
            .Returns(_fixture.Create<UserEntitlement>());

        var sut = new PullRequestApproverService(client, logQueryService);

        // Act
        var approvers = (await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>())).ToList();

        // Assert
        approvers.Count.ShouldBe(1);
        approvers.Single().ShouldBe(approver);
    }

    [Fact]
    public async Task GetAllApproversAsync_DataInLogAnalytics_AllValidApproversReturned()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        const string validApprover1 = "unittest@rabobank.com";
        const string validApprover2 = "test@rabobank.nl";
        const string inactiveApprover = "inactive@rabobank.nl";
        const string invalidApprover = "invalidmail@test.nl";

        var azdoClient = Substitute.For<IAzdoRestClient>();
        var build = _fixture.Create<Build>();
        build.Repository.Type = "TfsGit";

        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<Build>>(), Arg.Any<string>())
            .Returns(build);
        azdoClient
            .GetAsync(Arg.Is<IEnumerableRequest<UserEntitlement>>(x =>
                x.Request.QueryParams.Contains(
                    new KeyValuePair<string, object>("$filter", $"name eq '{validApprover1}'"))))
            .Returns(CreateUserEntitlement(validApprover1, true));
        azdoClient
            .GetAsync(Arg.Is<IEnumerableRequest<UserEntitlement>>(x =>
                x.Request.QueryParams.Contains(
                    new KeyValuePair<string, object>("$filter", $"name eq '{validApprover2}'"))))
            .Returns(CreateUserEntitlement(validApprover2, true));
        azdoClient
            .GetAsync(Arg.Is<IEnumerableRequest<UserEntitlement>>(x =>
                x.Request.QueryParams.Contains(
                    new KeyValuePair<string, object>("$filter", $"name eq '{inactiveApprover}'"))))
            .Returns(CreateUserEntitlement(inactiveApprover, false));
        azdoClient
            .GetAsync(Arg.Is<IEnumerableRequest<UserEntitlement>>(x =>
                x.Request.QueryParams.Contains(
                    new KeyValuePair<string, object>("$filter", $"name eq '{invalidApprover}'"))))
            .Returns(CreateUserEntitlement(invalidApprover, true));

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(
            new List<string> { validApprover1, validApprover2, inactiveApprover, invalidApprover },
            "test@test.nl", "test2@test.nl", "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var sut = new PullRequestApproverService(azdoClient, logQueryService);

        // Act
        var approvers = (await sut.GetAllApproversAsync(_fixture.Create<string>(), _fixture.Create<string>())).ToList();

        // Assert
        approvers.Count.ShouldBe(3);
        approvers.ShouldBe(new[] { validApprover1, validApprover2, inactiveApprover });
    }

    private static IEnumerable<UserEntitlement> CreateUserEntitlement(string principleName, bool active) =>
        new List<UserEntitlement>
        {
            new()
            {
                User = new User { PrincipalName = principleName },
                AccessLevel = new AccessLevel { Status = active ? "active" : "inactive" }
            }
        };

    [Fact]
    public async Task ShouldHaveNoApproverIfNoGitCommit()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());
        _fixture.Customize<Change>(x => x
            .With(c => c.Type, "XXX")
            .With(c => c.Id, "39b4c8d3acb868a956baeee248c3376fdc307ad3"));
        var client = Substitute.For<IAzdoRestClient>();
        client
            .GetAsync(Arg.Any<IEnumerableRequest<Change>>())
            .Returns(_fixture.CreateMany<Change>());
        client
            .GetAsync(Arg.Any<IEnumerableRequest<GitPullRequest>>())
            .Returns(_fixture.CreateMany<GitPullRequest>());
        var approvers = _fixture.CreateMany<IdentityRefWithVote>(3).ToList();
        approvers[0].UniqueName = "first approver";
        approvers[1].UniqueName = "second approver";
        approvers[2].UniqueName = "last approver";
        client
            .GetAsync(Arg.Any<IEnumerableRequest<IdentityRefWithVote>>())
            .Returns(approvers);

        var logQueryService = Substitute.For<ILogQueryService>();

        var sut = new PullRequestApproverService(client, logQueryService);

        // Act
        var hasApprover = await sut.HasApprovalAsync(_fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        hasApprover.ShouldBe(false);
    }

    [Fact]
    public async Task HasApprovalAsync_DataInLogAnalyticsPrApproverDifferentThanPrClosedBy_ReturnsTrue()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());

        var azdoClient = Substitute.For<IAzdoRestClient>();
        var build = _fixture.Create<Build>();
        build.Repository.Type = "TfsGit";

        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<Build>>(), Arg.Any<string>())
            .Returns(build);

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(new List<string> { "unittest@rabobank.com" }, "test@test.nl", "test2@test.nl",
            "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var sut = new PullRequestApproverService(azdoClient, logQueryService);

        // Act
        var hasApprover = await sut.HasApprovalAsync("1", "2", "raboweb-test");

        // Assert
        hasApprover.ShouldBeTrue();
    }

    [Fact]
    public async Task HasApprovalAsync_DataInLogAnalyticsPrApproverSameAsPrClosedBy_ReturnsFalse()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());

        var azdoClient = Substitute.For<IAzdoRestClient>();
        var build = _fixture.Create<Build>();
        build.Repository.Type = "TfsGit";

        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<Build>>(), Arg.Any<string>())
            .Returns(build);

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(new List<string> { "unittest@rabobank.com" }, "test@test.nl",
            "unittest@rabobank.com", "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var sut = new PullRequestApproverService(azdoClient, logQueryService);

        // Act
        var hasApprover = await sut.HasApprovalAsync("1", "2", "raboweb-test");

        // Assert
        hasApprover.ShouldBeFalse();
    }

    [Fact]
    public async Task HasApprovalAsync_DataInLogAnalyticsMultiplePrApprovers_ReturnsTrue()
    {
        // Arrange
        SetupFixture(_fixture.Create<string>(), _fixture.Create<string>());

        var azdoClient = Substitute.For<IAzdoRestClient>();
        var build = _fixture.Create<Build>();
        build.Repository.Type = "TfsGit";

        azdoClient
            .GetAsync(Arg.Any<IAzdoRequest<Build>>(), Arg.Any<string>())
            .Returns(build);

        var logQueryService = Substitute.For<ILogQueryService>();
        var result = CreateQueryResult(
            new List<string> { "unittest@rabobank.com", "test@test.nl", "test@rabobank.com" },
            "test@test.nl", "unittest2@rabobank.com", "1234");

        logQueryService
            .GetQueryEntryAsync<PullRequestApproveLogData>(Arg.Any<string>())
            .Returns(result);

        var sut = new PullRequestApproverService(azdoClient, logQueryService);

        // Act
        var hasApprover = await sut.HasApprovalAsync("1", "2", "raboweb-test");

        // Assert
        hasApprover.ShouldBeTrue();
    }

    private static PullRequestApproveLogData CreateQueryResult(IEnumerable<string> approvers, string createdBy,
        string? closedBy, string commitId) =>
        new()
        {
            Approvers = approvers,
            CreatedBy = createdBy,
            ClosedBy = closedBy,
            LastMergeCommitId = commitId
        };

    private void SetupFixture(string approverEmail, string userEmail, int vote = _approvedVote)
    {
        const string commitId = "39b4c8d3acb868a956baeee248c3376fdc307ad3";
        var identityId1 = Guid.NewGuid();
        var identityId2 = Guid.NewGuid();
        _fixture.Customize<Change>(customizationComposer => customizationComposer
            .With(change => change.Type, "TfsGit")
            .With(change => change.Id, commitId));
        _fixture.Customize<GitCommitRef>(customizationComposer => customizationComposer
            .With(gitCommitRef => gitCommitRef.CommitId, commitId));
        _fixture.Customize<IdentityRefWithVote>(customizationComposer => customizationComposer
            .With(identityRefWithVote => identityRefWithVote.Id, identityId1.ToString())
            .With(identityRefWithVote => identityRefWithVote.IsContainer, false)
            .With(identityRefWithVote => identityRefWithVote.Vote, vote)
            .With(identityRefWithVote => identityRefWithVote.UniqueName, approverEmail));
        _fixture.Customize<IdentityRef>(customizationComposer => customizationComposer
            .With(identityRef => identityRef.Id, identityId2));
        _fixture.Customize<User>(customizationComposer => customizationComposer
            .With(user => user.MailAddress, userEmail));
        _fixture.Customize<AccessLevel>(customizationComposer => customizationComposer
            .With(accessLevel => accessLevel.Status, "active"));
    }
}