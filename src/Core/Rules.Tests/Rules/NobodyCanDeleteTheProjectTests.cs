using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using ApplicationGroup = Rabobank.Compliancy.Infra.AzdoClient.Response.ApplicationGroup;
using Task = System.Threading.Tasks.Task;
using Rabobank.Compliancy.Core.Rules.Rules;
using Moq;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class NobodyCanDeleteTheProjectTests
{
    private readonly ApplicationGroup _pa = new ApplicationGroup
        { FriendlyDisplayName = "Project Administrators", TeamFoundationId = "1234" };

    private readonly ApplicationGroup _rpa = new ApplicationGroup
        { FriendlyDisplayName = "Rabobank Project Administrators", TeamFoundationId = "adgasge" };

    private readonly ApplicationGroup _cb = new ApplicationGroup
        { FriendlyDisplayName = "Contributor", TeamFoundationId = "3" };
        
    private readonly Permission _deleteTeamProjectAllow = new Permission
    {
        DisplayName = "Delete team project",
        PermissionBit = 4,
        PermissionId = PermissionLevelId.Allow,
        NamespaceId = SecurityNamespaceIds.Project,
        PermissionToken = "$PROJECT:vstfs:///Classification/TeamProject/53410703-e2e5-4238-9025-233bd7c811b3:"
    };

    private readonly Permission _deleteTeamProjectDeny = new Permission
    {
        DisplayName = "Delete team project",
        PermissionBit = 4,
        PermissionId = PermissionLevelId.Deny,
        NamespaceId = SecurityNamespaceIds.Project,
        PermissionToken = "$PROJECT:vstfs:///Classification/TeamProject/53410703-e2e5-4238-9025-233bd7c811b3:"
    };

    [Fact]
    public async Task GivenProjectAdministratorsMembersEmpty_WhenEvaluating_ThenTrue()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectDeny);
        InitializeApplicationGroupsLookup(client, _pa, _cb, _rpa);
        InitializeMembersLookup(client);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "")).ShouldBe(true);
    }

    [Fact]
    public async Task GivenProjectAdministratorsHasOnlyRabobankProjectAdministrators_WhenEvaluating_ThenTrue()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectDeny);
        InitializeApplicationGroupsLookup(client, _pa, _cb, _rpa);
        InitializeMembersLookup(client, _rpa);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "")).ShouldBe(true);
    }

    [Fact]
    public async Task GivenProjectAdministratorsHasOtherMember_WhenEvaluate_ThenFalse()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectDeny);
        InitializeApplicationGroupsLookup(client, _pa, _cb, _rpa);
        InitializeMembersLookup(client, new ApplicationGroup());

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "")).ShouldBe(false);
    }

    [Fact]
    public async Task GivenContributorsHasPermissionToDeleteTeamProject_WhenEvaluate_ThenFalse()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _cb);
        InitializeMembersLookup(client);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "")).ShouldBe(false);
    }

    [Fact]
    public async Task GivenProjectAdministratorsGroupContainsOtherMembers_WhenFix_ThenMembersAreRemoved()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa);
        InitializeMembersLookup(client,
            new ApplicationGroup { TeamFoundationId = "asdf" },
            new ApplicationGroup { TeamFoundationId = "gsdgs" });

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<SecurityManagement.EditMembersData, object>>(),
                Arg.Is<SecurityManagement.RemoveMembersData>(x =>
                    x.RemoveItemsJson.Contains("asdf") &&
                    x.RemoveItemsJson.Contains("gsdgs") &&
                    x.GroupId == "1234"),
                Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenProjectAdministratorsGroupContainsOtherMembers_WhenFix_ThenMembersAreAddedToRabobankProjectAdministratorsGroup()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa);
        InitializeMembersLookup(client,
            _rpa,
            new ApplicationGroup { TeamFoundationId = "asdf" },
            new ApplicationGroup { TeamFoundationId = "gsdgs" });

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<SecurityManagement.AddMemberData, object>>(),
                Arg.Is<SecurityManagement.AddMemberData>(x =>
                    x.GroupsToJoinJson.Contains(_rpa.TeamFoundationId) &&
                    x.ExistingUsersJson.Contains("asdf") &&
                    x.ExistingUsersJson.Contains("gsdgs")),
                Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenProjectAdministratorsGroupContainsRabobankAdministratorsGroups_WhenFix_ThenThatMemberIsNotRemoved()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa);
        InitializeMembersLookup(client, _rpa);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .DidNotReceive()
            .PostAsync(Arg.Any<IAzdoRequest<SecurityManagement.RemoveMembersData, object>>(),
                Arg.Any<SecurityManagement.RemoveMembersData>(), Arg.Any<string>());
    }


    [Fact]
    public async Task
        GivenProjectAdministratorsGroupProbablyDoesNotContainRabobankAdministratorsGroups_WhenFix_ThenThatGroupIsAdded()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa);
        InitializeMembersLookup(client);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<SecurityManagement.AddMemberData, object>>(),
                Arg.Is<SecurityManagement.AddMemberData>(x =>
                    x.ExistingUsersJson.Contains(_rpa.TeamFoundationId) &&
                    x.GroupsToJoinJson.Contains(_pa.TeamFoundationId)), 
                Arg.Any<string>());
    }

    [Fact]
    public async Task GivenRabobankProjectAdministratorsGroupExists_WhenFix_ThenThatGroupIsNotCreated()
    {
        // Arrange
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa);
        InitializeMembersLookup(client);

        // Act
        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        // Assert
        await client
            .DidNotReceive()
            .PostAsync(
                Arg.Any<IAzdoRequest<SecurityManagement.ManageGroupData, ApplicationGroup>>(),
                Arg.Any<SecurityManagement.ManageGroupData>(), Arg.Any<string>());
    }

    [Fact]
    public async Task GivenRabobankProjectAdministratorsGroupDoesNotExist_WhenFix_ThenThatGroupIsCreated()
    {
        // Arrange 
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa);
        InitializeMembersLookup(client);

        client
            .GetAsync(Arg.Any<IAzdoRequest<ApplicationGroups>>(), Arg.Any<string>())
            .Returns(x => new ApplicationGroups { Identities = new[] { _pa, _cb } },
                x => new ApplicationGroups { Identities = new[] { _pa, _cb, _rpa } });
        client
            .PostAsync(Arg.Any<IAzdoRequest<SecurityManagement.ManageGroupData, ApplicationGroup>>(),
                Arg.Any<SecurityManagement.ManageGroupData>(), Arg.Any<string>())
            .Returns(_rpa);

        // Act
        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        // Assert
        await client
            .Received()
            .PostAsync(
                Arg.Any<IAzdoRequest<SecurityManagement.ManageGroupData, ApplicationGroup>>(),
                Arg.Any<SecurityManagement.ManageGroupData>(), Arg.Any<string>());
    }

    [Fact]
    public async Task
        GivenRabobankProjectAdministratorsHasInheritedAllowToDeleteTeamProject_WhenFix_ThenPermissionsAreUpdated()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa, _cb);
        InitializeMembersLookup(client);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Is<Permissions.UpdateWrapper>(x =>
                    x.UpdatePackage.Contains(_rpa.TeamFoundationId) &&
                    x.UpdatePackage.Contains(@"PermissionBit"":4") &&
                    x.UpdatePackage.Contains(@"PermissionId"":2")), Arg.Any<string>());
    }

    [Fact]
    public async Task GivenOtherMembersHavePermissionsToDeleteTeamProject_WhenFix_ThenPermissionsAreUpdated()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializePermissions(client, _deleteTeamProjectAllow);
        InitializeApplicationGroupsLookup(client, _pa, _rpa,
            new ApplicationGroup { FriendlyDisplayName = "Contributors", TeamFoundationId = "afewsf" });
        InitializeMembersLookup(client);

        var rule = new NobodyCanDeleteTheProject(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("", "");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Is<Permissions.UpdateWrapper>(x =>
                    x.UpdatePackage.Contains("afewsf") &&
                    x.UpdatePackage.Contains(@"PermissionId"":0")), Arg.Any<string>());

        await client
            .DidNotReceive()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Is<Permissions.UpdateWrapper>(x =>
                    x.UpdatePackage.Contains(_pa.TeamFoundationId)), Arg.Any<string>());

        await client
            .Received(1)
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Is<Permissions.UpdateWrapper>(x =>
                    x.UpdatePackage.Contains(_rpa.TeamFoundationId)), Arg.Any<string>()); // Only the DENY update
    }

    private static void InitializeApplicationGroupsLookup(IAzdoRestClient client, params ApplicationGroup[] groups)
    {
        client
            .GetAsync(Arg.Is<IAzdoRequest<ApplicationGroups>>(x =>
                x.Resource.Contains("ReadScopedApplicationGroupsJson")), Arg.Any<string>())
            .Returns(new ApplicationGroups
            {
                Identities = groups
            });
    }

    private static void InitializeMembersLookup(IAzdoRestClient client, params ApplicationGroup[] members)
    {
        client
            .GetAsync(Arg.Is<IAzdoRequest<ApplicationGroups>>(x =>
                x.Resource.Contains("ReadGroupMembers")), Arg.Any<string>())
            .Returns(new ApplicationGroups
            {
                Identities = members
            });
    }

    private static void InitializePermissions(IAzdoRestClient client, params Permission[] permissions)
    {
        client.GetAsync(Arg.Any<IAzdoRequest<PermissionsProjectId>>(), Arg.Any<string>())
            .Returns(new PermissionsProjectId
                { Security = new PermissionsSet { Permissions = permissions } });
    }
}