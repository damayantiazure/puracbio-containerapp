using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Linq;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class PoliciesTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public PoliciesTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryRequiredReviewersPolicies()
    {
        var result = (await _client.GetAsync(Policies.RequiredReviewersPolicies(_config.ProjectId))).ToList();

        result.ShouldNotBeEmpty();
        result.Any(e => e.Id != 0).ShouldBeTrue();
        result.Any(e => e.IsBlocking).ShouldBeFalse();
        result.All(e => e.IsDeleted).ShouldBeFalse();
        result.All(e => e.IsEnabled).ShouldBeTrue();
        result.ShouldAllBe(e => e.Settings.RequiredReviewerIds.Count > 0);
        result.ShouldAllBe(e => !string.IsNullOrEmpty(e.Settings.Scope[0].MatchKind));
        result.ShouldAllBe(e => !string.IsNullOrEmpty(e.Settings.Scope[0].RefName));
        result.ShouldAllBe(e => e.Settings.Scope[0].RepositoryId != Guid.Empty);
    }

    [Fact]
    public async Task QueryMinimumNumberOfReviewersPolicies()
    {
        var result = (await _client.GetAsync(Policies.MinimumNumberOfReviewersPolicies(_config.ProjectId))).ToList();

        result.ShouldNotBeEmpty();
        result.Any(e => e.Id != 0).ShouldBeTrue();
        result.Any(e => e.IsBlocking).ShouldBeTrue();
        result.All(e => e.IsDeleted).ShouldBeFalse();
        result.Any(e => e.IsEnabled).ShouldBeTrue();

        result.Any(e => e.Settings.MinimumApproverCount != 0).ShouldBeTrue();
        result.All(e => e.Settings.AllowDownvotes).ShouldBeFalse();
        result.Any(e => e.Settings.CreatorVoteCounts).ShouldBeTrue();
        result.Any(e => e.Settings.ResetOnSourcePush).ShouldBeTrue();
        result.Any(e => !string.IsNullOrEmpty(e.Settings.Scope[0].MatchKind)).ShouldBeTrue();
        result.Any(e => e.Settings.Scope[0].RepositoryId != Guid.Empty).ShouldBeTrue();
        result.ShouldAllBe(e => e.Settings.Scope[0].RepositoryId != Guid.Empty);
    }

    [Fact]
    public async Task QueryPolicy()
    {
        var result = await _client.GetAsync(Policies.Policy(_config.ProjectName, Int32.Parse(_config.PolicyId)));
        result.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetAllPoliciesForProject()
    {
        var result = (await _client.GetAsync(Policies.All(_config.ProjectName))).ToList();

        result.ShouldNotBeEmpty();
        result.ShouldAllBe(e => e.Id != 0);
        result.Any(e => e.IsBlocking).ShouldBeTrue();
        result.All(e => e.IsDeleted).ShouldBeFalse();
        result.Any(e => e.IsEnabled).ShouldBeTrue();
    }

    [Fact]
    public async Task GetAllPoliciesConvertsToSpecific()
    {
        var policies = (await _client.GetAsync(Policies.All(_config.ProjectName))).ToList();

        policies.ShouldContain(p => p is RequiredReviewersPolicy);
        policies.ShouldContain(p => p is MinimumNumberOfReviewersPolicy);
    }
}