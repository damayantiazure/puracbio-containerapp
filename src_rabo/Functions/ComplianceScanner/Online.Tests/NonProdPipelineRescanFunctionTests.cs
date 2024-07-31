#nullable enable

using AutoFixture.AutoMoq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class NonProdPipelineRescanFunctionTests
{
    private readonly Mock<IAzdoRestClient> _azdoClient = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly Mock<IHttpContextAccessor> _contextAccessorMock = new();
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());
    private readonly Mock<IPipelineRegistrationRepository> _pipelineRegistrationRepositoryMock = new();
    private readonly Mock<IScanCiService> _scanCiServiceMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();

    [Fact]
    public async Task RunAsync_ValidInput_ShouldCallServiceCorrectly()
    {
        // Arrange
        const string organization = "testorg";
        const int pipelineId = 2;
        var projectId = Guid.NewGuid();
        var project = _fixture.Create<Project>();
        project.Id = projectId.ToString();
        var report = _fixture.Create<NonProdCompliancyReport>();
        var registrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        var httpRequest = _fixture.Create<HttpRequest>();

        _azdoClient
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _pipelineRegistrationRepositoryMock
            .Setup(x => x.GetAsync(organization, project.Id))
            .ReturnsAsync(registrations)
            .Verifiable();

        _scanCiServiceMock.Setup(x =>
                x.ScanNonProdPipelineAsync(organization, project, It.IsAny<DateTime>(), pipelineId.ToString(),
                    registrations))
            .ReturnsAsync(report)
            .Verifiable();

        _compliancyReportServiceMock.Setup(x => x.UpdateNonProdPipelineReportAsync(organization, project.Name, report))
            .Verifiable();

        var sut = new NonProdPipelineRescanFunction(_azdoClient.Object, _loggingServiceMock.Object,
            _scanCiServiceMock.Object, _compliancyReportServiceMock.Object, _pipelineRegistrationRepositoryMock.Object,
            _contextAccessorMock.Object, null);

        // Act
        var actual = await sut.ScanNonProdPipeline(
            new RescanPipelineRequest { Organization = organization, PipelineId = pipelineId, ProjectId = projectId },
            httpRequest);

        // Assert
        Assert.IsType<OkResult>(actual);
        _azdoClient.Verify();
        _loggingServiceMock.Verify();
        _scanCiServiceMock.Verify();
        _compliancyReportServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync_InvalidInput_ShouldThrowException()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        var rescanPipelineRequest = _fixture.Create<RescanPipelineRequest>();
        var httpRequest = httpContext.Request;
        _pipelineRegistrationRepositoryMock.Setup(x => x.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        var sut = new NonProdPipelineRescanFunction(_azdoClient.Object, _loggingServiceMock.Object,
            _scanCiServiceMock.Object, _compliancyReportServiceMock.Object, _pipelineRegistrationRepositoryMock.Object,
            _contextAccessorMock.Object, _securityContextMock.Object);

        // Act
        var actual = () => sut.ScanNonProdPipeline(rescanPipelineRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<string>()), Times.Once);
    }
}