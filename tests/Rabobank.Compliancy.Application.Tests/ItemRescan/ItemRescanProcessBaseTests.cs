using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.ProcessTestImplementations;
using Rabobank.Compliancy.Application.Tests.RequestImplementations;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infra.AzdoClient;
using Xunit;

namespace Rabobank.Compliancy.Application.Tests.ItemRescan;

public class ItemRescanProcessBaseTests
{
    protected readonly IFixture _fixture = new Fixture() { RepeatCount = 1 }.Customize(new IdentityIsAlwaysUser());

    protected readonly Mock<IAzdoRestClient> _azdoRestClient = new();
    protected readonly Mock<IRuleProcessor> _ruleProcessor = new();
    protected readonly Mock<IProjectService> _projectService = new();
    protected readonly Mock<ICompliancyReportService> _reportService = new();

    protected readonly ItemRescanProcessBaseTestImplementation _sut;

    public ItemRescanProcessBaseTests()
    {
        _sut = new ItemRescanProcessBaseTestImplementation(_ruleProcessor.Object, _projectService.Object, _reportService.Object);
    }


    [Fact]
    public void Constructor_Throws_OnNullRuleProcessor()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemRescanProcessBaseTestImplementation(null, _projectService.Object, _reportService.Object));
    }

    [Fact]
    public void Constructor_Throws_OnNullProjectService()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemRescanProcessBaseTestImplementation(_ruleProcessor.Object, null, _reportService.Object));
    }

    [Fact]
    public void Constructor_Throws_OnNullReportService()
    {
        Assert.Throws<ArgumentNullException>(() => new ItemRescanProcessBaseTestImplementation(_ruleProcessor.Object, _projectService.Object, null));
    }

    [Fact]
    public async Task GetParentProject_ReturnsExpectedProject_WhenCorrectParametersAreUsed()
    {
        // Arrange
        var firstOrg = _fixture.Create<string>();
        var secondOrg = _fixture.Create<string>();
        var returnableProject = _fixture.Create<Project>();

        _projectService.Setup(p => p.GetProjectByIdAsync(firstOrg, It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(returnableProject);

        // Act
        var firstProject = await _sut.UsesGetParentProject(firstOrg, returnableProject.Id);
        var secondProject = await _sut.UsesGetParentProject(secondOrg, returnableProject.Id);

        // Assert
        Assert.Equal(returnableProject.Name, firstProject.Name);
        Assert.Equal(returnableProject.Organization, firstProject.Organization);
        Assert.Equal(returnableProject.Id, firstProject.Id);

        Assert.Null(secondProject);
    }

    [Fact]
    public async Task UpdateReportAsync_CallsReportService_WithCorrectProject()
    {
        // Arrange
        var request = _fixture.Create<RuleRescanRequestBaseTestImplementation>();
        var itemProject = Setup_Default_GetProject_Mocks(request.Organization, request.ItemProjectId);
        var reportProject = Setup_Default_GetReportProject_Mocks(request);
        var evaluationResult = true;

        // Act
        await _sut.UsesUpdateReportAsync(request, evaluationResult);

        // Assert
        _reportService.Verify(r => r.UpdateComplianceStatusAsync(reportProject, request.ItemProjectId, request.ItemIdAsString, request.RuleName, request.ConcernsProjectRule, evaluationResult), Times.Once);
        _reportService.Verify(r => r.UpdateComplianceStatusAsync(itemProject, It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()), Times.Never);
        _projectService.Verify();
    }

    protected Project Setup_Default_GetProject_Mocks(string organization, Guid itemProjectId)
    {
        var project = _fixture.Create<Project>();
        project.Organization = organization;
        project.Id = itemProjectId;

        _projectService.Setup(p => p.GetProjectByIdAsync(organization, itemProjectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);

        return project;
    }

    protected Project Setup_Default_GetReportProject_Mocks(RuleRescanRequestBase request)
    {
        var reportProject = _fixture.Create<Project>();
        reportProject.Organization = request.Organization;
        reportProject.Id = request.ReportProjectId;

        _projectService.Setup(p => p.GetProjectByIdAsync(request.Organization, request.ReportProjectId, It.IsAny<CancellationToken>())).ReturnsAsync(reportProject)
            .Verifiable();

        return reportProject;
    }
}