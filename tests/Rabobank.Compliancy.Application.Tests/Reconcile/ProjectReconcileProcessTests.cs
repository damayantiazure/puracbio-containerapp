using Rabobank.Compliancy.Application.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;

namespace Rabobank.Compliancy.Application.Tests.Reconcile;

public class ProjectReconcileProcessTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IReconcileProcessor> _reconcileProcessor = new();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly ProjectReconcileProcess _sut;

    public ProjectReconcileProcessTests()
    {
        _sut = new ProjectReconcileProcess(_reconcileProcessor.Object, _projectServiceMock.Object, _compliancyReportServiceMock.Object);
        _fixture.Customize(new IdentityIsAlwaysUser());
    }

    [Fact]
    public void InitializeConstructor_WithoutRuleProcessor_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ProjectReconcileProcess> actual = () => new ProjectReconcileProcess(null, _projectServiceMock.Object, _compliancyReportServiceMock.Object);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void InitializeConstructor_WithoutProjectService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ProjectReconcileProcess> actual = () => new ProjectReconcileProcess(_reconcileProcessor.Object, null, _compliancyReportServiceMock.Object);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void InitializeConstructor_WithoutCompliancyReportService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ProjectReconcileProcess> actual = () => new ProjectReconcileProcess(_reconcileProcessor.Object, _projectServiceMock.Object, null);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public async Task HandleReconcileProcessAsync_WithNoProjectReconcileRule_ShouldNotExecuteUpdateReportAsync()
    {
        // Arrange
        var request = _fixture.Create<ReconcileRequest>();

        _reconcileProcessor.Setup(x => x.GetAllProjectReconcile())
            .Returns(Enumerable.Empty<IProjectReconcile>());

        // Act
        await _sut.ReconcileAsync(request, default);

        // Assert
        _projectServiceMock.Verify(x => x.GetProjectByIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public void HasRuleName_WithValidRuleName_ShouldReturnTrue()
    {
        // Arrange
        var request = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteTheProject)
            .Create();

        var projectReconcile = new Mock<IProjectReconcile>();
        projectReconcile.SetupGet(x => x.Name).Returns(RuleNames.NobodyCanDeleteTheProject);

        _reconcileProcessor.Setup(x => x.GetAllProjectReconcile())
            .Returns(new IProjectReconcile[] { projectReconcile.Object });

        // Act
        var actual = _sut.HasRuleName(request.RuleName);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public void HasRuleName_WithInValidRuleName_ShouldReturnFalse()
    {
        // Arrange
        var request = _fixture.Create<ReconcileRequest>();

        var projectReconcile = new Mock<IProjectReconcile>();
        projectReconcile.SetupGet(x => x.Name).Returns(RuleNames.NobodyCanDeleteTheProject);

        _reconcileProcessor.Setup(x => x.GetAllProjectReconcile())
            .Returns(new IProjectReconcile[] { projectReconcile.Object });

        // Act
        var actual = _sut.HasRuleName(request.RuleName);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleReconcileProcessAsync_WithProjectReconcileRule_ShouldExecuteUpdateReportAsync()
    {
        // Arrange
        var request = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteTheProject)
            .Create();

        var reevaluationResult = _fixture.Create<bool>();

        var projectReconcile = new Mock<IProjectReconcile>();
        projectReconcile.SetupGet(x => x.Name).Returns(RuleNames.NobodyCanDeleteTheProject);
        projectReconcile.Setup(x => x.ReconcileAndEvaluateAsync(request.Organization, request.ProjectId.ToString()))
            .ReturnsAsync(reevaluationResult)
            .Verifiable();

        _reconcileProcessor.Setup(x => x.GetAllProjectReconcile())
            .Returns(new IProjectReconcile[] { projectReconcile.Object });

        var project = _fixture.Create<Project>();
        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(request.Organization, request.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _compliancyReportServiceMock.Setup(x => x.UpdateComplianceStatusAsync(project, request.ProjectId, request.ItemId, request.RuleName, true, reevaluationResult))
            .Verifiable();

        // Act
        await _sut.ReconcileAsync(request, default);

        // Assert
        _projectServiceMock.Verify(x => x.GetProjectByIdAsync(request.Organization, request.ProjectId, It.IsAny<CancellationToken>()), Times.Once);
        _compliancyReportServiceMock
            .Verify(x => x.UpdateComplianceStatusAsync(project, request.ProjectId, request.ItemId, request.RuleName, true, reevaluationResult), Times.Once);
    }
}