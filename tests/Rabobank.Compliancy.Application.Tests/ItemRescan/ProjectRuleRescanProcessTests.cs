using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;
using Rabobank.Compliancy.Application.Tests.RuleImplementations;
using Rabobank.Compliancy.Core.Rules.Model;
using Xunit;

namespace Rabobank.Compliancy.Application.Tests.ItemRescan;

public class ProjectRuleRescanProcessTests : ItemRescanProcessBaseTests
{

    [Fact]
    public async Task RescanAndUpdateReportAsync_UpdatesReport_WithExpectedParameters()
    {
        var request = Setup_Default_RescanAndUpdateReportAsync_Mocks();
        var reportProject = Setup_Default_GetReportProject_Mocks(request);
        var sut = new ProjectRuleRescanProcessTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object);

        await sut.RescanAndUpdateReportAsync(request);

        _reportService.Verify(r => r.UpdateComplianceStatusAsync(reportProject, request.ItemProjectId, request.ItemProjectId.ToString(), request.RuleName, request.ConcernsProjectRule, true), Times.Once);
    }

    protected ProjectRuleRescanRequest Setup_Default_RescanAndUpdateReportAsync_Mocks()
    {
        var request = _fixture.Create<ProjectRuleRescanRequest>();
        var rule = new NobodyCanDeleteTheProjectTestThatReturnsTrueImplementation();
        request.RuleName = rule.Name;

        _ruleProcessor.Setup(r => r.GetRuleByName<IProjectRule>(request.RuleName)).Returns(rule);

        return request;
    }
}