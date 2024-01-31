using Rabobank.Compliancy.Application.Deviations;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Tests;
using Rabobank.Compliancy.Tests.Helpers;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Application.Tests.Deviations;

public class RegisterDeviationProcessTests : UnitTestBase
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly Mock<IDeviationService> _deviationServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IProjectService> _projectServiceMock = new();

    [Fact]
    public async Task RegisterDeviation_UpdatesReport_WithExpectedParameters()
    {
        // Arrange
        var request = _fixture.Create<RegisterDeviationRequest>();
        var project = new Project
        { Id = request.ProjectId, Name = InvariantUnitTestValue, Organization = request.Organization };
        _projectServiceMock
            .Setup(p => p.GetProjectByIdAsync(request.Organization, request.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);
        var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer");
        _authorizationServiceMock
            .Setup(p => p.GetCurrentUserAsync(request.Organization, authenticationHeaderValue,
                It.IsAny<CancellationToken>())).ReturnsAsync(new User { Username = InvariantUnitTestValue });
        _deviationServiceMock.SetupSequence(deviationService => deviationService.GetDeviationAsync(project,
                request.RuleName,
                request.ItemId, request.CiIdentifier, request.ForeignProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDeviation(request, project));

        var sut = new RegisterDeviationProcess(_authorizationServiceMock.Object, _projectServiceMock.Object,
            _deviationServiceMock.Object, _compliancyReportServiceMock.Object);

        // Act
        await sut.RegisterDeviation(request, authenticationHeaderValue);

        // Assert
        _compliancyReportServiceMock.Verify(r => r.AddDeviationToReportAsync(It.Is<Deviation>(deviation =>
            deviation.CiIdentifier == request.CiIdentifier &&
            deviation.Comment == request.Comment &&
            deviation.ItemId == request.ItemId &&
            deviation.ItemProjectId == request.ForeignProjectId &&
            deviation.Project.Id == request.ProjectId &&
            deviation.Project.Organization == request.Organization &&
            deviation.Reason == request.Reason &&
            deviation.ReasonNotApplicable == request.ReasonNotApplicable &&
            deviation.ReasonNotApplicableOther == request.ReasonNotApplicableOther &&
            deviation.ReasonOther == request.ReasonOther &&
            deviation.RuleName == request.RuleName)), Times.Once);
    }

    [Fact]
    public async Task RegisterDeviation_SendsLogRecord_WithExpectedParameters()
    {
        // Arrange
        var request = _fixture.Create<RegisterDeviationRequest>();
        var project = new Project { Id = request.ProjectId, Name = InvariantUnitTestValue, Organization = request.Organization };
        _projectServiceMock.Setup(p => p.GetProjectByIdAsync(request.Organization, request.ProjectId, It.IsAny<CancellationToken>())).ReturnsAsync(project);
        var authenticationHeaderValue = new AuthenticationHeaderValue("Bearer");
        _authorizationServiceMock.Setup(p => p.GetCurrentUserAsync(request.Organization, authenticationHeaderValue, It.IsAny<CancellationToken>())).ReturnsAsync(new User { Username = InvariantUnitTestValue });
        _deviationServiceMock.SetupSequence(deviationService => deviationService.GetDeviationAsync(project, request.RuleName,
            request.ItemId, request.CiIdentifier, request.ForeignProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateDeviation(request, project));

        var sut = new RegisterDeviationProcess(_authorizationServiceMock.Object, _projectServiceMock.Object, _deviationServiceMock.Object, _compliancyReportServiceMock.Object);

        // Act
        await sut.RegisterDeviation(request, authenticationHeaderValue);

        // Assert
        _deviationServiceMock.Verify(d => d.SendDeviationUpdateRecord(It.Is<Deviation>(deviation =>
            deviation.CiIdentifier == request.CiIdentifier &&
            deviation.Comment == request.Comment &&
            deviation.ItemId == request.ItemId &&
            deviation.ItemProjectId == request.ForeignProjectId &&
            deviation.Project.Id == request.ProjectId &&
            deviation.Project.Organization == request.Organization &&
            deviation.Reason == request.Reason &&
            deviation.ReasonNotApplicable == request.ReasonNotApplicable &&
            deviation.ReasonNotApplicableOther == request.ReasonNotApplicableOther &&
            deviation.ReasonOther == request.ReasonOther &&
            deviation.RuleName == request.RuleName), Domain.Compliancy.Deviations.DeviationReportLogRecordType.Insert)
            , Times.Once);
    }

    private static Deviation CreateDeviation(RegisterDeviationRequest registerDeviationRequest, Project project) =>
        new(registerDeviationRequest.ItemId!, registerDeviationRequest.RuleName!,
            registerDeviationRequest.CiIdentifier!, project, registerDeviationRequest.Reason,
            registerDeviationRequest.ReasonNotApplicable, registerDeviationRequest.ReasonOther,
            registerDeviationRequest.ReasonNotApplicableOther, registerDeviationRequest.Comment!)
        {
            ItemProjectId = registerDeviationRequest.ForeignProjectId
        };
}