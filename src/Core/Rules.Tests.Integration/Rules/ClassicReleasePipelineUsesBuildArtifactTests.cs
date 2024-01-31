using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class ClassicReleasePipelineUsesBuildArtifactTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public ClassicReleasePipelineUsesBuildArtifactTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("2", false)]
    [Trait("category", "integration")]
    public async Task EvaluateIntegrationTest(string pipelineId, bool testresult)
    {
        //Arrange
        var releasePipeline = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, pipelineId),
            _config.Organization);

        //Act
        var rule = new ClassicReleasePipelineUsesBuildArtifact(null);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, releasePipeline);

        //Assert
        result.ShouldBe(testresult);
    }
}