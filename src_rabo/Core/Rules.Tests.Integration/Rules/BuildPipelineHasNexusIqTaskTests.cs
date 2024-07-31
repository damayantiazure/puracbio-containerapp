using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class BuildPipelineHasNexusIqTaskTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly IYamlHelper _yamlHelper;

    public BuildPipelineHasNexusIqTaskTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
        _cache = Create.MockedMemoryCache();
        _yamlHelper = new YamlHelper(_cache, _client);
    }

    [Theory]
    [InlineData("505", true)]
    [InlineData("512", false)]
    [InlineData("507", true)]
    [InlineData("513", false)]
    [InlineData("506", false)]
    [Trait("category", "integration")]
    public async Task EvaluateBuildIntegrationTest(string pipelineId, bool expectedResult)
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, pipelineId),
            _config.Organization);

        var rule = new BuildPipelineHasNexusIqTask(_client, _cache, _yamlHelper);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);

        result.ShouldBe(expectedResult);
    }
}