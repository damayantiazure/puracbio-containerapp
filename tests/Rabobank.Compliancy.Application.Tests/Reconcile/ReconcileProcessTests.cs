using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Application.Tests.Reconcile;

public class ReconcileProcessTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IProjectReconcileProcess> _projectReconcileProcessMock = new();
    private readonly Mock<IItemReconcileProcess> _itemReconcileProcessMock = new();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();

    private readonly ReconcileProcess _sut;

    public ReconcileProcessTests()
    {
        _sut = new ReconcileProcess(_projectReconcileProcessMock.Object, _itemReconcileProcessMock.Object,
            _projectServiceMock.Object, _compliancyReportServiceMock.Object);
    }

    [Fact]
    public void InitializeConstructor_WithoutProjectReconcileProcess_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ReconcileProcess> actual = () => new ReconcileProcess(null, _itemReconcileProcessMock.Object,
            _projectServiceMock.Object, _compliancyReportServiceMock.Object);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void InitializeConstructor_WithoutItemReconcileProcess_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ReconcileProcess> actual = () => new ReconcileProcess(_projectReconcileProcessMock.Object, null,
            _projectServiceMock.Object, _compliancyReportServiceMock.Object);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void InitializeConstructor_WithoutProjectService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ReconcileProcess> actual = () => new ReconcileProcess(_projectReconcileProcessMock.Object, _itemReconcileProcessMock.Object,
            null, _compliancyReportServiceMock.Object);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void InitializeConstructor_WithoutCompliancyReportService_ShouldThrowArgumentNullException()
    {
        // Arrange & Act
        Func<ReconcileProcess> actual = () => new ReconcileProcess(_projectReconcileProcessMock.Object, _itemReconcileProcessMock.Object,
            _projectServiceMock.Object, null);

        // Assert
        actual.ShouldThrow<ArgumentNullException>();
    }

    [Fact]
    public void HasRuleName_WithValidRuleName_ShouldReturnTrue()
    {
        // Arrange
        var request = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds)
            .Create();

        _itemReconcileProcessMock.Setup(x => x.HasRuleName(RuleNames.NobodyCanDeleteBuilds)).Returns(true);

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

        // Act
        var actual = _sut.HasRuleName(request.RuleName);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task HandleReconcileProcessAsync_WithAnyRule_ShouldExecuteItemAndProjectHandleReconcileProcesAsync()
    {
        // Arrange
        var request = _fixture.Build<ReconcileRequest>()
            .With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds)
            .Create();

        // Act
        await _sut.ReconcileAsync(request, default);

        // Assert
        _projectReconcileProcessMock.Verify(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
        _itemReconcileProcessMock.Verify(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}