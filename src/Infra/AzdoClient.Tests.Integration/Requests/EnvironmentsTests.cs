using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

public class EnvironmentsTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public EnvironmentsTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Organization, _config.Token);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryEnvironments()
    {
        var environments = await _client.GetAsync(Environments.All(_config.ProjectName));
        environments.ShouldNotBeEmpty();

        var environment = environments.First();
        environment.Id.ShouldNotBe(0);
        environment.Name.ShouldNotBeNull();
        environment.Project.ShouldNotBeNull();
        environment.Project.Id.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryEnvironmentConfig()
    {
        var environments = await _client.GetAsync(Environments.All(_config.ProjectName));
        var environmentId = environments.OrderBy(e => e.Id).First().Id;
        var configs = await _client.GetAsync(Environments.Configuration(_config.ProjectName, environmentId));
        configs.ShouldNotBeEmpty();

        var config = configs.First();
        config.Id.ShouldNotBe(0);
        config.Resource.ShouldNotBeNull();
        config.Resource.Id.ShouldBe(environmentId);
        config.Type.ShouldNotBeNull();
        config.Type.Name.ShouldNotBeNull();
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task QueryEnvironmentSecurityGroups()
    {
        var projectId = (await _client.GetAsync(Project.ProjectById(_config.ProjectName)).ConfigureAwait(false)).Id;
        var environments = await _client.GetAsync(Environments.All(_config.ProjectName));
        var environmentId = environments.First().Id;
        var groups = await _client.GetAsync(Environments.Security(projectId, environmentId));
        groups.ShouldNotBeEmpty();

        var group = groups.First();
        group.Identity.ShouldNotBeNull();
        group.Identity.DisplayName.ShouldNotBeNull();
        group.Role.ShouldNotBeNull();
        group.Role.Name.ShouldNotBeNull();
    }
}