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

public class ReleasePipelineServiceTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IBuildPipelineService _buildService;
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();
    private readonly Mock<IYamlHelper> _yamlHelper = new Mock<IYamlHelper>();

    public ReleasePipelineServiceTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
        _buildService = new BuildPipelineService(_client, _cache, _yamlHelper.Object);
    }

    [Fact]
    public async Task AllLinkedPipelinesAndRepositoriesFromClassicReleasePipelineAreDetectedAsync()
    {
        // Arrange
        var releasePipeline = await _client.GetAsync(Requests.ReleaseManagement.Definition(_config.Project, "2"),
            _config.Organization);
        var defaultProfile = new DefaultRuleProfile();

        // Act
        var service = new ReleasePipelineService(_client, _buildService, _cache);

        var linkedPipelines = await service.GetLinkedPipelinesAsync(_config.Organization, releasePipeline, _config.Project);
        var linkedRepos = await service.GetLinkedRepositoriesAsync(_config.Organization, new List<ReleaseDefinition> { releasePipeline }, linkedPipelines);

        // Assert
        linkedPipelines.ShouldBe(new[]
        {
            new BuildDefinition { Name = "compliant-classic-build-pipeline", Id = "505", ProfileToApply = defaultProfile},
            new BuildDefinition { Name = "build-artifacts", Id = "523", ProfileToApply = defaultProfile },
            new BuildDefinition { Name = "compliant-yaml-pipeline-with-approve-task", Id = "634", ProfileToApply = defaultProfile }
        }, ignoreOrder: true);

        // Innersource repo will not be found. Project originates from another organization.
        // Cross-organization resources are not supported.
        linkedRepos.Select(r => r.Name).ShouldBe(new[]
        {
            "integration-tests (resources)",
            "integration-tests",
            "Azdo-pipeline-templates"
        }, ignoreOrder: true);
    }

    [Fact]
    public async Task ClassicReleasePipelineWithInvalidDownloadTasksShouldReturnNullAsync()
    {
        // Arrange
        var releasePipeline = await _client.GetAsync(Requests.ReleaseManagement.Definition(_config.Project, "3"),
            _config.Organization);

        // Act
        var service = new ReleasePipelineService(_client, _buildService, _cache);

        var linkedPipelines = await service.GetLinkedPipelinesAsync(_config.Organization, releasePipeline, _config.Project);
        var linkedRepos = await service.GetLinkedRepositoriesAsync(_config.Organization, new List<ReleaseDefinition> { releasePipeline }, linkedPipelines);

        // Assert
        linkedPipelines.Count().ShouldBe(0);
        linkedRepos.Count().ShouldBe(0);
    }
}