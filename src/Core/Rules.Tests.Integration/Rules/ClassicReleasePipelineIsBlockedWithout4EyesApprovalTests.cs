#nullable enable

using NSubstitute;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Core.Rules.Tests.Integration.Rules;

public class ClassicReleasePipelineIsBlockedWithout4EyesApprovalTests : IClassFixture<TestConfig>
{
    private readonly TestConfig _config;
    private readonly IAzdoRestClient _client;

    public ClassicReleasePipelineIsBlockedWithout4EyesApprovalTests(TestConfig config)
    {
        _config = config;
        _client = new AzdoRestClient(_config.Token);
    }

    [Theory]
    [InlineData("1","1", true)]
    [InlineData("2","2", false)]
    [Trait("category", "integration")]
    public async Task EvaluateIntegrationTest(string pipelineId, string stageId, bool testResult)
    {
        //Arrange
        var releasePipeline = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, pipelineId),
            _config.Organization);

        var productionItems = Substitute.For<IPipelineRegistrationResolver>();
        productionItems.ResolveProductionStagesAsync(_config.Organization, _config.Project, releasePipeline.Id)
            .Returns(new[] { stageId });
        var ruleConfig = new RuleConfig { ValidateGatesHostName = "https://validategatesdev.azurewebsites.net" };

        //Act
        var rule = new ClassicReleasePipelineIsBlockedWithout4EyesApproval(_client, productionItems, ruleConfig);
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, releasePipeline);

        //Assert
        Assert.Equal(testResult, result);
    }

    [SuppressMessage("Code Smell", "xUnit1004:Test methods should not be skipped.", Justification = "For manual testing only.")]
    [Fact(Skip = "For manual testing only")]
    [Trait("category", "integration")]
    public async Task Reconcile()
    {
        //Arrange
        var releasePipelineBefore = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, "3"),
            _config.Organization);

        var productionItems = Substitute.For<IPipelineRegistrationResolver>();
        productionItems.ResolveProductionStagesAsync(_config.Organization, _config.Project, releasePipelineBefore.Id)
            .Returns(new[] { "3" });
        var ruleConfig = new RuleConfig { ValidateGatesHostName = "https://validategatesdev.azurewebsites.net" };

        //Act
        var rule = new ClassicReleasePipelineIsBlockedWithout4EyesApproval(_client, productionItems, ruleConfig);
        await rule.ReconcileAsync(_config.Organization, _config.Project, "3");

        var releasePipelineAfter = await _client.GetAsync(ReleaseManagement.Definition(_config.Project, "3"));
        var result = await rule.EvaluateAsync(_config.Organization, _config.Project, releasePipelineAfter);

        //Assert
        Assert.True(result);
    }
}