using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Permission = Rabobank.Compliancy.Infra.AzdoClient.Requests.Permissions;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("Category", "integration")]
public class PermissionsTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public PermissionsTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryPermissionsGroupRepositoryReturnsPermissions()
    {
        var applicationGroupId = (await _client.GetAsync(ApplicationGroup.ApplicationGroups(_config.ProjectName))).Identities
            .First(gi => gi.DisplayName == $"[{_config.ProjectName}]\\Project Administrators").TeamFoundationId;

        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;

        var repositories = await _client.GetAsync(Repository.Repositories(_config.ProjectName));

        foreach (var repository in repositories)
        {
            var request = Permission.PermissionsGroupRepository(projectId, applicationGroupId, repository.Id);
            var permissionsGitRepository = await _client.GetAsync(request);

            permissionsGitRepository.ShouldNotBeNull();
            permissionsGitRepository.Permissions.First().ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task QueryPermissionsGroupSetIdReturnsPermissionsForSetId()
    {
        var permissionSetId = (await _client.GetAsync(SecurityNamespace.SecurityNamespaces()))
            .First(ns => ns.Name == "Build").NamespaceId;

        var applicationGroupId = (await _client.GetAsync(ApplicationGroup.ApplicationGroups(_config.ProjectName))).Identities
            .First(gi => gi.DisplayName == $"[{_config.ProjectName}]\\Project Administrators").TeamFoundationId;

        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;

        var permissionsGroupSetId = await _client.GetAsync(
            Permission.PermissionsGroupSetId(projectId, permissionSetId, applicationGroupId)
            );

        permissionsGroupSetId.ShouldNotBeNull();
        permissionsGroupSetId.CurrentTeamFoundationId.ShouldNotBeNull();
        permissionsGroupSetId.Permissions.First().ShouldNotBeNull();
    }

    [Fact]
    public async Task QueryPermissionsGroupSetIdDefinitionReturnsPermissionsForSetId()
    {
        var permissionSetId = (await _client.GetAsync(SecurityNamespace.SecurityNamespaces()))
            .First(ns => ns.Name == "Build").NamespaceId;

        var applicationGroupId = (await _client.GetAsync(ApplicationGroup.ApplicationGroups(_config.ProjectName))).Identities
            .First(gi => gi.DisplayName == $"[{_config.ProjectName}]\\Project Administrators").TeamFoundationId;

        var projectId = (await _client.GetAsync(Project.Properties(_config.ProjectName))).Id;
        var buildDefinitions = await _client.GetAsync(Builds.BuildDefinitions(projectId));

        foreach (var buildDefinition in buildDefinitions)
        {
            var permissionsGroupSetId = (await _client.GetAsync(Permission.PermissionsGroupSetIdDefinition(
                projectId, permissionSetId, applicationGroupId, $"{projectId}/{buildDefinition.Id}")));

            permissionsGroupSetId.ShouldNotBeNull();
        }
    }

    [Fact]
    public async Task SetPermissionsAsync()
    {
        await _client.PostAsync(Permission.ManagePermissions(_config.ProjectName),
            new ManagePermissionsData(
                "2c12fa83-5bdb-4085-a635-c7cd00cdfba5",
                "S-1-9-1551374245-50807123-3856808002-2418352955-3620213171-1-1337613045-2794958661-2397635820-2543327080",
                "Microsoft.TeamFoundation.Identity", "vstfs:///Classification/TeamProject/53410703-e2e5-4238-9025-233bd7c811b3",
                new Response.Permission
                {
                    PermissionId = PermissionLevelId.Deny,
                    PermissionBit = ProjectBits.DeleteProject,
                    NamespaceId = SecurityNamespaceIds.Project,
                    PermissionToken = "$PROJECT:vstfs:///Classification/TeamProject/53410703-e2e5-4238-9025-233bd7c811b3:"
                }
            ).Wrap());
    }
}