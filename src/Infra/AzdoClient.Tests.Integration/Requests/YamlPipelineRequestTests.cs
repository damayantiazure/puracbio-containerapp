using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class YamlPipelineRequestTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public YamlPipelineRequestTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(config.Organization, config.Token);
    }

    [Fact]
    public async Task CanValidateYamlPipeline()
    {
        var response = await _client.PostAsync(YamlPipeline.Parse(_config.ProjectId, "507"),
            new YamlPipeline.YamlPipelineRequest());
        Assert.NotNull(response.FinalYaml);
    }

    [Fact]
    public async Task CanParseYamlPipelineWithRevision()
    {
        var response = await _client.PostAsync(YamlPipeline.Parse(
                _config.ProjectId, _config.BuildDefinitionIdYaml, _config.BuildPipelineRevision),
            new YamlPipeline.YamlPipelineRequest(), _config.Organization, true);
        Assert.NotNull(response.FinalYaml);
    }
}