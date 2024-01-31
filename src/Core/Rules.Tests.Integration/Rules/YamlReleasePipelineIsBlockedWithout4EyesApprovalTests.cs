using Moq;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using Shouldly;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class YamlReleasePipelineIsBlockedWithout4EyesApprovalTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public YamlReleasePipelineIsBlockedWithout4EyesApprovalTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Theory]
    [InlineData("507", new[] { "Prod"}, true)]
    [InlineData("506", new string[] {}, false)]
    [InlineData("513", new[] { "Prod", "Prod2" }, false)]
    [Trait("category", "integration")]
    public async Task Evaluate_IntegrationTest(string pipelineId, string[] stages, bool testResult)
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, pipelineId),
            _config.Organization);
        var resolver = new Mock<IPipelineRegistrationResolver>();
        resolver.Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(stages);
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(_client, resolver.Object);
        var ruleConfig = new RuleConfig { ValidateGatesHostName = "validategatesdev.azurewebsites.net" };

        var rule = new YamlReleasePipelineIsBlockedWithout4EyesApproval(_client, yamlEnvironmentHelper, ruleConfig);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);
        result.ShouldBe(testResult);
    }

    [Fact]
    [Trait("category", "integration")]
    public async Task ReconcileIntegrationTest()
    {
        var buildPipeline = await _client.GetAsync(Builds.BuildDefinition(_config.Project, "514"),
            _config.Organization);
        var resolver = new Mock<IPipelineRegistrationResolver>();
        resolver.Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] {"Prod"});
        var yamlEnvironmentHelper = new YamlEnvironmentHelper(_client, resolver.Object);
        var ruleConfig = new RuleConfig { ValidateGatesHostName = "validategatesdev.azurewebsites.net" };
        var rule = new YamlReleasePipelineIsBlockedWithout4EyesApproval(_client, yamlEnvironmentHelper, ruleConfig);
        await rule.ReconcileAsync(_config.Organization, _config.Project, buildPipeline.Id);

        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, buildPipeline);
        result.ShouldBe(true);
    }
}