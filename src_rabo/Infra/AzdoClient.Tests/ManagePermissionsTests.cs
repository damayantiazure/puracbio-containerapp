using AutoFixture;
using AutoFixture.AutoNSubstitute;
using NSubstitute;
using Shouldly;
using Xunit;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public static class ManagePermissionsTests
{
    [Fact]
    public static async Task Update()
    {
        // Arrange 
        var group = new ApplicationGroup();
        var permission = new Permission();
        var permissions = new PermissionsSet { Permissions = new[] { permission, } };

        var mock = Substitute.For<ISecurityNamespaceContext>();
        mock
            .GetAllApplicationGroupsWithExplicitPermissions()
            .Returns(new ApplicationGroups { Identities = new[] { group } });
        mock
            .GetPermissionSetForApplicationGroup(group.TeamFoundationId)
            .Returns(permissions);

        // Act
        var target = new ManagePermissions(mock);
        await target.UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received()
            .UpdatePermissionForGroupAsync(group.TeamFoundationId, permissions, permission);

        permission
            .PermissionId
            .ShouldBe(4);
    }

    [Fact]
    public static async Task Ignore()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<ApplicationGroup>(ctx => ctx
            .With(x => x.FriendlyDisplayName, "some-name"));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        var target = new ManagePermissions(mock);
        await target.SetApplicationGroupDisplayNamesToIgnore("some-name").UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received(0)
            .UpdatePermissionForGroupAsync(Arg.Any<string>(), Arg.Any<PermissionsSet>(), Arg.Any<Permission>());
    }

    [Fact]
    public static async Task For()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<ApplicationGroup>(ctx => ctx
            .With(x => x.FriendlyDisplayName, "some-name"));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        await new ManagePermissions(mock).SetApplicationGroupsInScopeByDisplayName("some-name").UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received()
            .UpdatePermissionForGroupAsync(Arg.Any<string>(), Arg.Any<PermissionsSet>(), Arg.Any<Permission>());
    }

    [Fact]
    public static async Task Permissions()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionBit, 1234));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        var target = new ManagePermissions(mock);
        await target.SetPermissionsToBeInScope(1234).UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received()
            .UpdatePermissionForGroupAsync(Arg.Any<string>(), Arg.Any<PermissionsSet>(), Arg.Any<Permission>());
    }

    [Fact]
    public static async Task PermissionsIgnoreOther()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionBit, 1));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        var target = new ManagePermissions(mock);
        await target.SetPermissionsToBeInScope(1234).UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received(0)
            .UpdatePermissionForGroupAsync(Arg.Any<string>(), Arg.Any<PermissionsSet>(), Arg.Any<Permission>());
    }

    [Fact]
    public static async Task Allow()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionId, 1));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        var target = new ManagePermissions(mock);
        await target.SetPermissionLevelIdsThatAreOkToHave(1).UpdatePermissionsInScopeForGroupsInScopeAsync(4);

        // Assert
        await mock
            .Received(0)
            .UpdatePermissionForGroupAsync(Arg.Any<string>(), Arg.Any<PermissionsSet>(), Arg.Any<Permission>());
    }

    [Fact]
    public static async Task Validate()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionId, 4)
            .With(x => x.PermissionBit, 1234));

        var mock = fixture.Create<ISecurityNamespaceContext>();        // Act
        var target = new ManagePermissions(mock);
        var result = await target
            .SetPermissionsToBeInScope(1324)
            .SetPermissionLevelIdsThatAreOkToHave(4)
            .ValidateAsync();

        // Assert
        result.ShouldBe(true);
    }

    [Fact]
    public static async Task ValidateFalse()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionId, 1)
            .With(x => x.PermissionBit, 1234));

        var mock = fixture.Create<ISecurityNamespaceContext>();        // Act
        var target = new ManagePermissions(mock);
        var result = await target
            .SetPermissionsToBeInScope(1234)
            .SetPermissionLevelIdsThatAreOkToHave(4)
            .ValidateAsync();

        // Assert
        result.ShouldBe(false);
    }

    [Fact]
    public static async Task ValidateIgnore()
    {
        // Arrange             
        var client = Substitute.For<IAzdoRestClient>();
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<ApplicationGroup>(ctx => ctx
            .With(x => x.FriendlyDisplayName, "some-name"));

        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionId, 1)
            .With(x => x.PermissionBit, 1234));

        var mock = fixture.Create<ISecurityNamespaceContext>();

        // Act
        var target = new ManagePermissions(mock);
        var result = await target
            .SetApplicationGroupDisplayNamesToIgnore("some-name")
            .SetPermissionsToBeInScope(1234)
            .SetPermissionLevelIdsThatAreOkToHave(4)
            .ValidateAsync();

        // Assert
        await client
            .Received(0)
            .GetAsync(Arg.Any<IAzdoRequest<PermissionsSet>>(), Arg.Any<string>());
        result.ShouldBeFalse();
    }

    [Fact]
    public static async Task ValidateOtherPermission()
    {
        // Arrange 
        var fixture = new Fixture().Customize(new AutoNSubstituteCustomization { ConfigureMembers = true });
        fixture.Customize<ApplicationGroup>(ctx => ctx
            .With(x => x.FriendlyDisplayName, "some-name"));

        fixture.Customize<Permission>(ctx => ctx
            .With(x => x.PermissionId, 1)
            .With(x => x.PermissionBit, 1234));

        var mock = fixture.Create<ISecurityNamespaceContext>();        // Act
        var target = new ManagePermissions(mock);
        var result = await target
            .SetPermissionsToBeInScope(4444)
            .SetPermissionLevelIdsThatAreOkToHave(4)
            .ValidateAsync();

        // Assert
        result.ShouldBe(true);
    }
}