using AutoFixture;
using AutoFixture.AutoNSubstitute;
using MemoryCache.Testing.Moq;
using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System.Collections.Generic;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.AzureDevOpsGroups;
using ApplicationGroup = Rabobank.Compliancy.Infra.AzdoClient.Response.ApplicationGroup;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class NobodyCanDeleteTheRepositoryTests
{
    private const string RepositoryId = "3167b64e-c72b-4c55-84eb-986ac62d0dec";


    [Fact]
    public async Task GivenAnApplicationGroupHasPermissionToDeleteRepoWithAllow_WhenEvaluating_ThenFalse()
    {
        var client = Substitute.For<IAzdoRestClient>();

        InitializeLookupData(client, PermissionLevelId.Allow);

        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "", RepositoryId)).ShouldBe(false);
    }

    [Fact]
    public async Task GivenAnApplicationGroupHasPermissionToDeleteRepoWithAllowInHerited_WhenEvaluating_ThenFalse()
    {
        var client = Substitute.For<IAzdoRestClient>();

        InitializeLookupData(client, PermissionLevelId.AllowInherited);

        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "", RepositoryId)).ShouldBe(false);
    }

    [Fact]
    public async Task GivenNoApplicationGroupHasPermissionToDeleteRepo_WhenEvaluating_ThenTrue()
    {
        var client = Substitute.For<IAzdoRestClient>();

        InitializeLookupData(client, PermissionLevelId.Deny);

        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "", RepositoryId)).ShouldBe(true);
    }

    [Fact]
    public async Task IgnoreGroupsProjectCollectionAdminAndProjectCollectionServiceAccounts()
    {
        var client = Substitute.For<IAzdoRestClient>();

        var applicationGroup1 = new ApplicationGroup
        {
            FriendlyDisplayName = ProjectCollectionAdministrators,
            DisplayName = "blblblablaaProject Collection Administrators",
            TeamFoundationId = "11"
        };

        var applicationGroup2 = new ApplicationGroup
        {
            FriendlyDisplayName = "Project Collection Service Accounts",
            DisplayName = "blblblablaaProject Collection Service Accounts",
            TeamFoundationId = "22"
        };

        var applicationGroup3 = new ApplicationGroup
        {
            FriendlyDisplayName = "Dit is een test",
            DisplayName = "blblblablaaDit is een testy",
            TeamFoundationId = "33"
        };

        var applicationGroups = new ApplicationGroups
        { Identities = new[] { applicationGroup1, applicationGroup2, applicationGroup3 } };

        InitializeLookupData(client, PermissionLevelId.Deny);

        client.GetAsync(Arg.Any<IAzdoRequest<ApplicationGroups>>(), Arg.Any<string>())
            .Returns(applicationGroups);

        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        (await rule.EvaluateAsync("", "", RepositoryId)).ShouldBe(true);


        await client
            .DidNotReceive()
            .GetAsync(Arg.Is<IAzdoRequest<PermissionsSet>>(x =>
                x.QueryParams.Contains(new KeyValuePair<string, object>("tfid", "11"))), Arg.Any<string>());

        await client
            .DidNotReceive()
            .GetAsync(Arg.Is<IAzdoRequest<PermissionsSet>>(x =>
                x.QueryParams.Contains(new KeyValuePair<string, object>("tfid", "22"))), Arg.Any<string>());

        await client
            .Received()
            .GetAsync(Arg.Is<IAzdoRequest<PermissionsSet>>(x =>
                x.QueryParams.Contains(new KeyValuePair<string, object>("tfid", "33"))), Arg.Any<string>());
    }

    [Fact]
    public async Task GivenPermissionIsAllowWhenFixPermissionIsUpdatedToDeny()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializeLookupData(client, PermissionLevelId.Allow);

        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("raboweb", "TAS", "123");

        await client
            .Received()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Is<Permissions.UpdateWrapper>(x =>
                    x.UpdatePackage.Contains("123") &&
                    x.UpdatePackage.Contains(@"PermissionId"":2")),
                Arg.Any<string>());
    }

    [Fact]
    public async Task GivenPermissionIsDeny_WhenFixPermission_IsNotUpdated()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializeLookupData(client, PermissionLevelId.Deny);


        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("raboweb", "TAS", "123");

        await client
            .DidNotReceive()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Any<Permissions.UpdateWrapper>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task GivenPermissionIsInheritedDeny_WhenFixPermission_IsNotUpdated()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializeLookupData(client, PermissionLevelId.DenyInherited);


        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("raboweb", "TAS", "123");

        await client
            .DidNotReceive()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Any<Permissions.UpdateWrapper>(),
                Arg.Any<string>());
    }

    [Fact]
    public async Task GivenPermissionIsNotSet_WhenFixPermission_IsNotUpdated()
    {
        var client = Substitute.For<IAzdoRestClient>();
        InitializeLookupData(client, PermissionLevelId.NotSet);


        var rule = new NobodyCanDeleteTheRepository(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync("raboweb", "TAS", "123");

        await client
            .DidNotReceive()
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(),
                Arg.Any<Permissions.UpdateWrapper>(),
                Arg.Any<string>());
    }

    private static void InitializeLookupData(IAzdoRestClient client, int permissionId)
    {
        var fixture = new Fixture();
        fixture.Customize(new AutoNSubstituteCustomization());

        client.GetAsync(Arg.Any<IAzdoRequest<ProjectProperties>>(), Arg.Any<string>())
            .Returns(fixture.Create<ProjectProperties>());
        client.GetAsync(Arg.Any<IAzdoRequest<ApplicationGroups>>(), Arg.Any<string>())
            .Returns(fixture.Create<ApplicationGroups>());

        client.GetAsync(Arg.Any<IAzdoRequest<PermissionsSet>>(), Arg.Any<string>())
            .Returns(new PermissionsSet
            {
                Permissions = new[]
                {
                    new Permission
                    {
                        DisplayName = "Delete repository", PermissionBit = 512, PermissionId = permissionId,
                        PermissionToken = "repoV2/53410703-e2e5-4238-9025-233bd7c811b3/123"
                    }
                }
            });
    }
}