using System;
using System.Linq;
using System.Threading.Tasks;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class SecurityManagementTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public SecurityManagementTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task ReadGroupMembers()
    {


        var groupId = (await _client
                .GetAsync(SecurityManagement.Groups(_config.ProjectId)))
            .Identities
            .Single(x => x.FriendlyDisplayName == "Project Administrators")
            .TeamFoundationId;

        Guid.TryParse(groupId,  out _).ShouldBeTrue();

        var groupMembers = await _client
            .GetAsync(SecurityManagement.GroupMembers(_config.ProjectId, groupId));
            
        groupMembers.TotalIdentityCount.ShouldNotBe(0);
    }

    [Fact]
    public async Task AddAndRemoveGroupMemberAsync()
    {

        var groupId = (await _client
                .GetAsync(SecurityManagement.Groups(_config.ProjectId)))
            .Identities
            .Single(x => x.FriendlyDisplayName == "Project Administrators")
            .TeamFoundationId;
            
        await _client.PostAsync(
            SecurityManagement.AddMember(_config.ProjectName), 
            new SecurityManagement.AddMemberData(
                new []{ "ab84d5a2-4b8d-68df-9ad3-cc9c8884270c" }, 
                new [] { groupId }));

        await _client.PostAsync(
            SecurityManagement.EditMembership(_config.ProjectName),
            new SecurityManagement.RemoveMembersData(new[] { "ab84d5a2-4b8d-68df-9ad3-cc9c8884270c"}, groupId));
    }

    [Fact(Skip = "unable to delete created group with API")]
    public async Task CreateGroup()
    {
        await _client.PostAsync(
            SecurityManagement.ManageGroup(_config.ProjectName),
            new SecurityManagement.ManageGroupData
            {
                Name = "asdf"
            });
    }
}