using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class YamlReleasePipelineHasSm9ChangeTaskTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache;
    private readonly IYamlHelper _yamlHelper;

    public YamlReleasePipelineHasSm9ChangeTaskTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
        _cache = Create.MockedMemoryCache();
        _yamlHelper = new YamlHelper(_cache, _client);

    }

    [Theory]
    [InlineData("507", true)]
    [InlineData("634", true)]
    [InlineData("506", false)]
    [InlineData("513", false)]
    [Trait("category", "integration")]
    public async Task EvaluateIntegrationTest(string pipelineId, bool testResult)
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, pipelineId),
            _config.Organization);
        var rule = new YamlReleasePipelineHasSm9ChangeTask(_client, _yamlHelper);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);
        result.ShouldBe(testResult);
    }
}