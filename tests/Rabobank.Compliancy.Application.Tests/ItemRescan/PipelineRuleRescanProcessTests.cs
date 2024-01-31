using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;
using Rabobank.Compliancy.Application.Tests.RuleImplementations;
using Rabobank.Compliancy.Core.Rules.Model;
using Xunit;

namespace Rabobank.Compliancy.Application.Tests.ItemRescan;

public class PipelineRuleRescanProcessTests : ItemRescanProcessBaseTests
{
    protected readonly Mock<IPipelineService> _pipelineService = new();

    [Fact]
    public void Constructor_Throws_OnNullPipelineService()
    {
        Assert.Throws<ArgumentNullException>(() => new PipelineRuleRescanProcessTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object, null));
    }

    [Fact]
    public async Task RescanAndUpdateReportAsync_UpdatesReport_WithExpectedParameters()
    {
        var request = Setup_Default_RescanAndUpdateReportAsync_Mocks();
        var reportProject = Setup_Default_GetReportProject_Mocks(request);
        var sut = new PipelineRuleRescanProcessTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object, _pipelineService.Object);
        var cancellationToken = _fixture.Create<CancellationToken>();

        await sut.RescanAndUpdateReportAsync(request, cancellationToken);

        _reportService.Verify(r => r.UpdateComplianceStatusAsync(reportProject, request.ItemProjectId, request.PipelineId.ToString(), request.RuleName, request.ConcernsProjectRule, true), Times.Once);
    }

    protected PipelineRuleRescanRequest Setup_Default_RescanAndUpdateReportAsync_Mocks()
    {
        var request = _fixture.Create<PipelineRuleRescanRequest>();
        var rule = new IClassicReleasePipelineRuleTestThatReturnsTrueImplementation(_azdoRestClient.Object);
        request.RuleName = rule.Name;

        _ruleProcessor.Setup(r => r.GetRuleByName<IPipelineRule>(request.RuleName)).Returns(rule);

        return request;
    }
}