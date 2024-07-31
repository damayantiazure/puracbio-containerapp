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
using Shouldly;
using System;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class ProjectRescanFunctionTests : FunctionRequestTests
{
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly IFixture _fixture = new Fixture { RepeatCount = 1 };
    private readonly Mock<IScanProjectService> _scanProjectServiceMock = new();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(x => x.Validate(TestRequest, organization, projectId))
            .Throws(exception)
            .Verifiable();

        // Act
        var function = new ProjectRescanFunction(_azdoClientMock.Object, _validateInputServiceMock.Object,
            _scanProjectServiceMock.Object, _loggingServiceMock.Object);
        var result = await function.RunAsync(TestRequest, organization, projectId);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(ProjectRescanFunction) &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId),
            exception)
        );
    }

    [Fact]
    public async Task CiScanErrors_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var ciIdentifier = _fixture.Create<string>();
        var exception = _fixture.Create<ExceptionSummaryReport>();
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<CiReport>(c =>
            c.FromFactory<string>(f => new CiReport(ciIdentifier, f, scanDate))
                .With(x => x.IsScanFailed, true)
                .With(x => x.ScanException, exception));

        var complianceReport = _fixture.Create<CompliancyReport>();

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();
        _scanProjectServiceMock
            .Setup(x => x.ScanProjectAsync(organization, project, It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(complianceReport)
            .Verifiable();

        // Act
        var function = new ProjectRescanFunction(_azdoClientMock.Object, _validateInputServiceMock.Object,
            _scanProjectServiceMock.Object, _loggingServiceMock.Object);
        var result = await function.RunAsync(request, organization, projectId);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        var resultValue = ((BadRequestObjectResult)result).Value!.ToString();
        resultValue!.ShouldContain("One or more CI scans failed. All CI failures have been logged to Log Analytics.");
        resultValue!.ShouldContain(
            @$"The first failure was for CI: {ciIdentifier} with Exception: {exception.ExceptionMessage}.");

        _azdoClientMock.Verify();
        _scanProjectServiceMock.Verify();
    }

    [Fact]
    public async Task ValidInputAndNoCiScanErrors_ShouldReturn_OkResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var project = _fixture.Create<Project>();

        _fixture.Customize<CiReport>(c => c
            .With(x => x.IsScanFailed, false));

        var complianceReport = _fixture.Create<CompliancyReport>();

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();
        _scanProjectServiceMock
            .Setup(x => x.ScanProjectAsync(organization, project, It.IsAny<DateTime>(), It.IsAny<int>()))
            .ReturnsAsync(complianceReport)
            .Verifiable();

        // Act
        var function = new ProjectRescanFunction(_azdoClientMock.Object, _validateInputServiceMock.Object,
            _scanProjectServiceMock.Object, _loggingServiceMock.Object);
        var result = await function.RunAsync(request, organization, projectId);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
        _azdoClientMock.Verify();
        _scanProjectServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync_ThrowsException_ShouldThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        _validateInputServiceMock
            .Setup(x => x.Validate(TestRequest, organization, projectId))
            .Throws<InvalidOperationException>();

        var sut = new ProjectRescanFunction(_azdoClientMock.Object, _validateInputServiceMock.Object,
            _scanProjectServiceMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = () => sut.RunAsync(TestRequest, organization, projectId);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<InvalidOperationException>()));
    }
}