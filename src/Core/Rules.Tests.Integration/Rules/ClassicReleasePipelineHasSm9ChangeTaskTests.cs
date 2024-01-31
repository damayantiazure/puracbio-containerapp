using System.Threading.Tasks;
using MemoryCache.Testing.Moq;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class ClassicReleasePipelineHasSm9ChangeTaskTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;
    private readonly IMemoryCache _cache = Create.MockedMemoryCache();

    public ClassicReleasePipelineHasSm9ChangeTaskTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Theory]
    [InlineData("1", true)]
    [InlineData("4", true)]
    [InlineData("2", false)]
    [Trait("category", "integration")]
    public async Task EvaluateIntegrationTest(string pipelineId, bool testResult)
    {
        //Arrange
        var releasePipeline = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, pipelineId),
            _config.Organization);

        //Act
        var rule = new ClassicReleasePipelineHasSm9ChangeTask(_client, _cache);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, releasePipeline);

        //Assert
        result.ShouldBe(testResult);
    }
}