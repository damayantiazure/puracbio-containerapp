using FluentAssertions;
using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Infrastructure.Parsers;
using Rabobank.Compliancy.Tests.Helpers;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests;

[Trait("category", "integration")]
[Collection("ProjectInitFixture")]
public class AccessControlListsRepositoryIntegrationTests
{
    private readonly ProjectInitFixture _fixture;

    public AccessControlListsRepositoryIntegrationTests(ProjectInitFixture fixture) => _fixture = fixture;

    [Fact]
    public async Task GetAccessControlListsForProjectAndSecurityNamespaceAsync_Returns_CorrectProjectPermissions()
    {
        var accessControlListsRepo = _fixture.ServiceProvider.GetServiceOrThrow<IAccessControlListsRepository>();
        var identityRepo = _fixture.ServiceProvider.GetServiceOrThrow<IIdentityRepository>();

        var project = await _fixture.GetProjectAsync();

        var acls = (await accessControlListsRepo.GetAccessControlListsForProjectAndSecurityNamespaceAsync(_fixture.Organization, project.Id, new Guid("52d39943-cb85-4d7f-8fa8-c6baac873819")))?.ToList();

        acls.Should().NotBeNull();
        var acesDictionaries = acls!.SelectMany(acl => acl.AcesDictionary).ToList();
        var descriptors = acesDictionaries.Select(ad => ad.Key).ToList();
        descriptors.Should().NotBeNull();
        var identities = (await identityRepo.GetIdentitiesForIdentityDescriptorsAsync(_fixture.Organization, descriptors, Microsoft.VisualStudio.Services.Identity.QueryMembership.Direct))?.ToList();
        identities.Should().NotBeNull();
        var paGroup = identities!.Single(i => i.DisplayName == $"[{project.Name}]\\Project Administrators");
        var rpaGroup = identities!.Single(i => i.DisplayName == $"[{project.Name}]\\Rabobank Project Administrators");
        var paAcesDictionary = acesDictionaries.Single(ad => ad.Key == paGroup.Descriptor);
        var rpaAcesDictionary = acesDictionaries.Single(ad => ad.Key == rpaGroup.Descriptor);

        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.GENERIC_READ).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.GENERIC_WRITE).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.DELETE).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.PUBLISH_TEST_RESULTS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.ADMINISTER_BUILD).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.START_BUILD).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.EDIT_BUILD_STATUS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.UPDATE_BUILD).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.DELETE_TEST_RESULTS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.VIEW_TEST_RESULTS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.MANAGE_TEST_ENVIRONMENTS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.MANAGE_TEST_CONFIGURATIONS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.WORK_ITEM_DELETE).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.WORK_ITEM_MOVE).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.WORK_ITEM_PERMANENTLY_DELETE).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.RENAME).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.MANAGE_PROPERTIES).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.BYPASS_RULES).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.SUPPRESS_NOTIFICATIONS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.UPDATE_VISIBILITY).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(paAcesDictionary.Value.Allow, ProjectPermissionBits.CHANGE_PROCESS).Should().BeTrue();
        PermissionsBitFlagsParser.IsEnumFlagPresent(rpaAcesDictionary.Value.Deny, ProjectPermissionBits.DELETE).Should().BeTrue(); // Should be denied for RPA.
    }
}