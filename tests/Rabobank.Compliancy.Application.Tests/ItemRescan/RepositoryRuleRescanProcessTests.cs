using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;
using Rabobank.Compliancy.Application.Tests.RuleImplementations;
using Rabobank.Compliancy.Core.Rules.Model;
using Xunit;

namespace Rabobank.Compliancy.Application.Tests.ItemRescan;

public class RepositoryRuleRescanProcessTests : ItemRescanProcessBaseTests
{
    protected readonly Mock<IGitRepoService> _gitRepoService = new();

    [Fact]
    public void Constructor_Throws_OnNullGitRepoService()
    {
        Assert.Throws<ArgumentNullException>(() => new RepositoryRuleRescanProcessTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object, null));
    }

    [Fact]
    public async Task RescanAndUpdateReportAsync_UpdatesReport_WithExpectedParameters()
    {
        var request = Setup_Default_RescanAndUpdateReportAsync_Mocks();
        var reportProject = Setup_Default_GetReportProject_Mocks(request);
        var sut = new RepositoryRuleRescanProcessTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object, _gitRepoService.Object);

        await sut.RescanAndUpdateReportAsync(request);

        _reportService.Verify(r => r.UpdateComplianceStatusAsync(reportProject, request.ItemProjectId, request.GitRepoId.ToString(), request.RuleName, request.ConcernsProjectRule, true), Times.Once);
        _ruleProcessor.Verify();
    }

    protected RepositoryRuleRescanRequest Setup_Default_RescanAndUpdateReportAsync_Mocks()
    {
        var request = _fixture.Create<RepositoryRuleRescanRequest>();
        var rule = new NobodyCanDeleteTheRepositoryTestThatReturnsTrueImplementation();
        request.RuleName = rule.Name;

        _ruleProcessor.Setup(r => r.GetRuleByName<IRepositoryRule>(request.RuleName)).Returns(rule).Verifiable();

        return request;
    }
}