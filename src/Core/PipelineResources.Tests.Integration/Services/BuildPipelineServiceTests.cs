using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Requests = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Integration.Services;

public class BuildPipelineServiceTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();
    private readonly IYamlHelper _yamlHelper = new Mock<IYamlHelper>().Object;

    public BuildPipelineServiceTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Fact]
    public async Task AllLinkedPipelinesAndRepositoriesFromYamlPipelineAreDetectedAsync()
    {
        // Arrange
        var buildPipeline = await _client.GetAsync(Requests.Builds.BuildDefinition(_config.Project, "507"),
            _config.Organization);
        var defaultProfile = new DefaultRuleProfile();

        // Act
        var buildService = new BuildPipelineService(_client, _cache, _yamlHelper);

        var linkedPipelines = await buildService.GetLinkedPipelinesAsync(_config.Organization, buildPipeline);
        var linkedRepos = await buildService.GetLinkedRepositoriesAsync(_config.Organization,
            linkedPipelines.Concat(new List<BuildDefinition> { buildPipeline }).ToList());

        // Assert
        linkedPipelines.ShouldBe(new[]
        {
            new BuildDefinition { Name = "incompliant-classic-build-pipeline", Id = "512", ProfileToApply = defaultProfile },
            new BuildDefinition { Name = "YamlBuildPipeline", Id = "674", ProfileToApply = defaultProfile},
            new BuildDefinition { Name = "PipelineThatDownloadsAnArtifact", Id = "673", ProfileToApply = defaultProfile },
            new BuildDefinition { Name = "Starter pipeline", Id = "672", ProfileToApply = defaultProfile }
        }, ignoreOrder: true);

        // Innersource repo will not be found. Project originates from another organization.
        // Cross-organization resources are not supported.

        linkedRepos.Select(r => r.Name).ShouldBe(new[]
        {
            "integration-tests (resources)",
            "integration-tests",
            "PowerBI",
            "HelloWorld",
            "NestedYamlTemplates",
            "Azdo-pipeline-templates"
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task YamlPipelineWithInvalidDownloadTasksShouldReturnNullAsync()
    {
        // Arrange
        var buildPipeline = await _client.GetAsync(Requests.Builds.BuildDefinition(_config.Project, "675"),
            _config.Organization);

        // Act
        var buildService = new BuildPipelineService(_client, _cache, _yamlHelper);

        var linkedPipelines = await buildService.GetLinkedPipelinesAsync(_config.Organization, buildPipeline);
        var linkedRepos = await buildService.GetLinkedRepositoriesAsync(_config.Organization,
            linkedPipelines.Concat(new List<BuildDefinition> { buildPipeline }).ToList());

        // Assert
        linkedPipelines.Count().ShouldBe(0);
        linkedRepos.Count().ShouldBe(1);
        linkedRepos.First().ShouldBe(buildPipeline.Repository);
    }
}