using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using Xunit;
using MemoryCache.Testing.Moq;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.AzureDevOpsGroups;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class NobodyCanDeleteReleasesTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });

    [Theory, CombinatorialData]
    public async Task EvaluateFalse(
        [CombinatorialValues(PermissionLevelId.Allow, PermissionLevelId.AllowInherited)] int permissionId,
        [CombinatorialValues(
            ReleaseDefinitionBits.DeleteReleases,
            ReleaseDefinitionBits.AdministerReleasePermissions,
            ReleaseDefinitionBits.DeleteReleasePipelines)] int permissionBit)
    {
        // Arrange 
        CustomizePermission(permissionId, permissionBit);

        // Act
        var rule = new NobodyCanDeleteReleases(_fixture.Create<IAzdoRestClient>(), Create.MockedMemoryCache());
        var result = await rule.EvaluateAsync(_fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<Response.ReleaseDefinition>());

        // Assert
        result.ShouldBeFalse();
    }

    [Theory, CombinatorialData]
    public async Task EvaluateTrue(
        [CombinatorialValues(PermissionLevelId.Deny, PermissionLevelId.DenyInherited, PermissionLevelId.NotSet)] int permissionId,
        [CombinatorialValues(
            ReleaseDefinitionBits.DeleteReleases,
            ReleaseDefinitionBits.AdministerReleasePermissions,
            ReleaseDefinitionBits.DeleteReleasePipelines)] int permissionBit)
    {
        // Arrange 
        CustomizePermission(permissionId, permissionBit);

        // Act
        var rule = new NobodyCanDeleteReleases(_fixture.Create<IAzdoRestClient>(), Create.MockedMemoryCache());
        var result = await rule.EvaluateAsync(_fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<Response.ReleaseDefinition>());

        // Assert
        result.ShouldBeTrue();
    }

    [Theory]
    [InlineData(ProjectCollectionAdministrators)]
    public async Task Exclude(string group)
    {
        // Arrange
        CustomizePermission(PermissionLevelId.Allow, ReleaseDefinitionBits.AdministerReleasePermissions);
        CustomizeApplicationGroup(group);
        var client = _fixture.Create<IAzdoRestClient>();

        // Act
        var rule = new NobodyCanDeleteReleases(client, Create.MockedMemoryCache());
        var result = await rule.EvaluateAsync(_fixture.Create<string>(), _fixture.Create<string>(),
            _fixture.Create<Response.ReleaseDefinition>());

        // Assert
        await client
            .Received(0)
            .GetAsync(Arg.Any<IAzdoRequest<Response.PermissionsSet>>(), Arg.Any<string>());
        result.ShouldBeFalse();
    }

    [Fact]
    public async Task Reconcile()
    {
        // Arrange
        CustomizePermission(PermissionLevelId.Allow, ReleaseDefinitionBits.DeleteReleases);
        var client = _fixture.Create<IAzdoRestClient>();

        // Act
        var rule = new NobodyCanDeleteReleases(client, Create.MockedMemoryCache());
        await rule.ReconcileAsync(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<string>());

        // Assert
        await client
            .Received(_fixture.RepeatCount * _fixture.RepeatCount) // identities * permissions
            .PostAsync(Arg.Any<IAzdoRequest<Permissions.UpdateWrapper, object>>(), 
                Arg.Any<Permissions.UpdateWrapper>(), Arg.Any<string>());
    }

    private void CustomizeApplicationGroup(string group) =>
        _fixture.Customize<Response.ApplicationGroup>(ctx => ctx
            .With(x => x.FriendlyDisplayName, @group));

    private void CustomizePermission(int permissionId, int permissionBit) =>
        _fixture.Customize<Response.Permission>(ctx => ctx
            .With(x => x.PermissionId, permissionId)
            .With(x => x.PermissionBit, permissionBit));
}