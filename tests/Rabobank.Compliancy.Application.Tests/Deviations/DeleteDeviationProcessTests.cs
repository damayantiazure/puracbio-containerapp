using Rabobank.Compliancy.Application.Deviations;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;

namespace Rabobank.Compliancy.Application.Tests.Deviations;

public class DeleteDeviationProcessTests
{
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock;
    private readonly Mock<IDeviationService> _deviationServiceMock;

    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IProjectService> _projectServiceMock;
    private readonly DeleteDeviationProcess _sut;

    public DeleteDeviationProcessTests()
    {
        _fixture.Customize(new IdentityIsAlwaysUser());
        _projectServiceMock = new Mock<IProjectService>();
        _deviationServiceMock = new Mock<IDeviationService>();
        _compliancyReportServiceMock = new Mock<ICompliancyReportService>();

        _sut = new DeleteDeviationProcess(_projectServiceMock.Object, _deviationServiceMock.Object,
            _compliancyReportServiceMock.Object);
    }

    [Fact]
    public async Task DeleteDeviationAsync_WithNoProjectFound_ShouldThrowSourceItemNotFoundException()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        var exceptionMessage = _fixture.Create<string>();
        _projectServiceMock.Setup(x =>
                x.GetProjectByIdAsync(deleteDeviationRequest.Organization, deleteDeviationRequest.ProjectId, default))
            .ThrowsAsync(new SourceItemNotFoundException(exceptionMessage));

        // Act
        var actual = async () => await _sut.DeleteDeviationAsync(deleteDeviationRequest);

        // Assert
        await actual.ShouldThrowAsync<SourceItemNotFoundException>();
        _deviationServiceMock.Verify(x => x.DeleteDeviationAsync(It.IsAny<Deviation>(), default), Times.Never);
    }

    [Fact]
    public async Task DeleteDeviationAsync_WithNoRegisteredDeviation_ShouldRemoveDeviationFromTheDeviationService()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        var project = _fixture.Create<Project>();
        _projectServiceMock.Setup(x =>
                x.GetProjectByIdAsync(deleteDeviationRequest.Organization, deleteDeviationRequest.ProjectId, default))
            .ReturnsAsync(project);

        _deviationServiceMock.Setup(x => x.GetDeviationAsync(project, deleteDeviationRequest.RuleName,
                deleteDeviationRequest.ItemId
                , deleteDeviationRequest.CiIdentifier, deleteDeviationRequest.ForeignProjectId, default))
            .ReturnsAsync((Deviation)null);

        // Act
        await _sut.DeleteDeviationAsync(deleteDeviationRequest);

        // Assert
        _projectServiceMock.Verify(
            x => x.GetProjectByIdAsync(deleteDeviationRequest.Organization, deleteDeviationRequest.ProjectId, default),
            Times.Once);
        _deviationServiceMock.Verify(x => x.DeleteDeviationAsync(It.IsAny<Deviation>(), default), Times.Never);
        _compliancyReportServiceMock.Verify(x => x.RemoveDeviationFromReportAsync(It.IsAny<Deviation>()), Times.Never);
    }

    [Fact]
    public async Task DeleteDeviationAsync_WithRegisteredDeviation_ShouldDeleteTheRegisteredDeviation()
    {
        // Arrange
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        var project = _fixture.Build<Project>()
            .With(x => x.Id, deleteDeviationRequest.ProjectId)
            .With(x => x.Organization, deleteDeviationRequest.Organization)
            .Create();

        var comment = _fixture.Create<string>();

        _fixture.Customize<Deviation>(ctx => ctx
            .FromFactory<string>(name => new Deviation(deleteDeviationRequest.ItemId, deleteDeviationRequest.RuleName,
                deleteDeviationRequest.CiIdentifier,
                project, null, null, null, null, comment)
            ).With(x => x.ItemProjectId, deleteDeviationRequest.ForeignProjectId));

        var deviation = _fixture.Create<Deviation>();

        _projectServiceMock.Setup(x =>
                x.GetProjectByIdAsync(deleteDeviationRequest.Organization, deleteDeviationRequest.ProjectId, default))
            .ReturnsAsync(project)
            .Verifiable();

        _deviationServiceMock.Setup(x => x.GetDeviationAsync(project, deleteDeviationRequest.RuleName,
                deleteDeviationRequest.ItemId
                , deleteDeviationRequest.CiIdentifier, deleteDeviationRequest.ForeignProjectId, default))
            .ReturnsAsync(deviation)
            .Verifiable();

        _compliancyReportServiceMock.Setup(x => x.RemoveDeviationFromReportAsync(It.IsAny<Deviation>())).Verifiable();

        // Act
        await _sut.DeleteDeviationAsync(deleteDeviationRequest);

        // Assert
        Func<Deviation, bool> verifyDeviation = (Deviation x) =>
            x.Project.Id == deleteDeviationRequest.ProjectId &&
            x.Project.Organization == deleteDeviationRequest.Organization &&
            x.RuleName == deleteDeviationRequest.RuleName &&
            x.ItemId == deleteDeviationRequest.ItemId &&
            x.CiIdentifier == deleteDeviationRequest.CiIdentifier;

        _projectServiceMock.Verify();
        _deviationServiceMock.Verify();
        _deviationServiceMock.Verify(x => x.DeleteDeviationAsync(It.Is<Deviation>(x => verifyDeviation(x))
            , default), Times.Once);
        _compliancyReportServiceMock.Verify();

        _deviationServiceMock.Verify(x => x.SendDeviationUpdateRecord(It.Is<Deviation>(x => verifyDeviation(x))
            , Domain.Compliancy.Deviations.DeviationReportLogRecordType.Delete), Times.Once);
    }
}