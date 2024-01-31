using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class ApplicationGroupTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public ApplicationGroupTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task QueryApplicationGroupsOrganization()
    {
        var identity = await _client.GetAsync(ApplicationGroup.ApplicationGroups());
        identity.ShouldNotBeNull();
        identity.Identities.ShouldNotBeEmpty();

        var group = identity.Identities.First();
        group.DisplayName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task QueryApplicationGroupsProject()
    {
        var identity = await _client.GetAsync(ApplicationGroup.ApplicationGroups(_config.ProjectName));
        identity.ShouldNotBeNull();
        identity.Identities.ShouldNotBeEmpty();

        var group = identity.Identities.First();
        group.DisplayName.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExplicitIdentitiesForReposShouldGetIdentities()
    {

        var explicitIdentities = await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesRepos(_config.ProjectId, 
            SecurityNamespaceIds.GitRepositories, _config.RepositoryId));
        explicitIdentities.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExplicitIdentitiesForReposRootFolderShouldGetIdentities()
    {
        var explicitIdentities = await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesRepos(_config.ProjectId, 
            SecurityNamespaceIds.GitRepositories));
        explicitIdentities.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExplicitIdentitiesForBranchShouldGetIdentities()
    {
        var explicitIdentities = await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesMasterBranch(_config.ProjectId, 
            SecurityNamespaceIds.GitRepositories, _config.RepositoryId));
        explicitIdentities.ShouldNotBeNull();
    }

    [Fact]
    public async Task ExplicitIdentitiesForBuildDefinitionShouldGetIdentities()
    {

        var explicitIdentities = await _client.GetAsync(ApplicationGroup.ExplicitIdentitiesPipelines(_config.ProjectId, 
            SecurityNamespaceIds.Build, _config.BuildDefinitionIdYaml));
        explicitIdentities.ShouldNotBeNull();
        explicitIdentities.Identities.ShouldNotBeEmpty();
    }
}