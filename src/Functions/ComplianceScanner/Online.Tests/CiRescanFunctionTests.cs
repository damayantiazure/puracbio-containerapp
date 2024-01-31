#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Shouldly;
using System;
using System.Linq;
using System.Net.Http;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class CiRescanFunctionTests : FunctionRequestTests
{
    private readonly Mock<IAzdoRestClient> _azdoClient = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IPipelineRegistrationRepository> _registrationRepoMock = new();
    private readonly Mock<IScanCiService> _scanCiServiceMock = new();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(x => x.Validate(organization, projectId, ciIdentifier, TestRequest))
            .Throws(exception)
            .Verifiable();

        // Act
        var function = new CiRescanFunction(_azdoClient.Object, _validateInputServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _registrationRepoMock.Object, _loggingServiceMock.Object);
        var result = await function.RunAsync(TestRequest, organization, projectId, ciIdentifier);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(CiRescanFunction) &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId), exception, ciIdentifier)
        );
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var ciIdentifier = _fixture.Create<string>();

        _validateInputServiceMock
            .Setup(x => x.Validate(organization, projectId, ciIdentifier, TestRequest))
            .Throws<InvalidOperationException>();

        var sut = new CiRescanFunction(_azdoClient.Object, _validateInputServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _registrationRepoMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = () => sut.RunAsync(TestRequest, organization, projectId, ciIdentifier);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ValidInputShouldReturnOkResult()
    {
        // Arrange
        _fixture.Customize<BuildDefinition>(c => c
            .With(p => p.PipelineType, ItemTypes.YamlPipelineWithStages));

        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _fixture.Create<Guid>().ToString)
            .Create();
        var ciIdentifier = _fixture.Create<string>();
        var ciReport = _fixture.Create<CiReport>();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _scanCiServiceMock
            .Setup(x => x.ScanCiAsync(organization, project, ciIdentifier, It.IsAny<DateTime>(), registrations))
            .ReturnsAsync(ciReport)
            .Verifiable();

        _registrationRepoMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();

        _compliancyReportServiceMock
            .Setup(x => x.UpdateCiReportAsync(organization, Guid.Parse(project.Id), project.Name, ciReport,
                It.IsAny<DateTime>()))
            .Verifiable();

        // Act
        var function = new CiRescanFunction(_azdoClient.Object, _validateInputServiceMock.Object, _scanCiServiceMock.Object,
            _compliancyReportServiceMock.Object, _registrationRepoMock.Object, _loggingServiceMock.Object);
        var result = await function.RunAsync(request, organization, project.Id, ciIdentifier);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock.Verify();
    }
}