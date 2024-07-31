#nullable enable

using System;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;
using Task = System.Threading.Tasks.Task;
using User = Rabobank.Compliancy.Domain.Compliancy.Authorizations.User;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class RegisterPipelineFunctionTests : FunctionRequestTests
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IPipelineRegistrator> _registratorMock = new();
    private readonly RegisterPipelineFunction _sut;
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();

    public RegisterPipelineFunctionTests() =>
        _sut = new RegisterPipelineFunction(_azdoClientMock.Object, _registratorMock.Object,
            _validateInputServiceMock.Object, _authorizationServiceMock.Object, _compliancyReportServiceMock.Object,
            _loggingServiceMock.Object);

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(x => x.Validate(TestRequest, organization, projectId, pipelineId))
            .Throws(exception)
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(TestRequest, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(RegisterPipelineFunction) &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId), exception, pipelineId, It.IsAny<string>(),
            It.IsAny<string>())
        );
    }

    [Fact]
    public async Task UnauthorizedUser_ShouldReturn_UnauthorizedResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;

        _authorizationServiceMock
            .Setup(x => x.HasEditPermissionsAsync(request, organization, projectId, pipelineId, pipelineType))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(UnauthorizedObjectResult));
        _authorizationServiceMock.Verify();
    }

    [Fact]
    public async Task PipelineNotFound_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var request = _fixture.Build<HttpRequestMessage>()
            .Without(x => x.Content).Create();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;

        _authorizationServiceMock
            .Setup(x => x.HasEditPermissionsAsync(request, organization, projectId, pipelineId, pipelineType))
            .ThrowsAsync(new ItemNotFoundException(ErrorMessages.ItemNotFoundError))
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        ((ObjectResult)result).Value!.ToString()
            .ShouldStartWith("An error occurred while retrieving the permissions for this pipeline");
        _authorizationServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync__ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var request = _fixture.Build<HttpRequestMessage>()
            .Without(x => x.Content).Without(x => x.RequestUri).Create();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(x => x.Validate(request, organization, projectId, pipelineId))
            .Throws(exception)
            .Verifiable();

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(RegisterPipelineFunction) &&
                e.RequestUrl == null &&
                e.Organization == organization &&
                e.ProjectId == projectId), exception, pipelineId, It.IsAny<string>(),
            It.IsAny<string>())
        );
    }

    [Fact]
    public async Task RunAsync_NonProdPipelineRegistration_ShouldReturnOKResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var input = new RegistrationRequest
        {
            CiIdentifier = null,
            Environment = null,
            Profile = null
        };
        var httpContent = new { input.CiIdentifier, input.Environment };
        var jsonContent = JsonConvert.SerializeObject(httpContent);
        request.Content = new StringContent(jsonContent);

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _authorizationServiceMock
            .Setup(x => x.HasEditPermissionsAsync(request, organization, project.Id, pipelineId, pipelineType))
            .ReturnsAsync(true);

        _registratorMock
            .Setup(x => x.RegisterNonProdPipelineAsync(organization, project.Id, pipelineId, pipelineType,
                It.IsAny<string>()))
            .ReturnsAsync(new OkResult())
            .Verifiable();

        _compliancyReportServiceMock
            .Setup(x => x.UpdateRegistrationAsync(organization, project.Name,
                pipelineId, pipelineType, It.IsAny<string>()))
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, project.Id, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
        _registratorMock.Verify(x => x.RegisterProdPipelineAsync(It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>(), input), Times.Never);
        _registratorMock.Verify();
        _compliancyReportServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync_ProdPipelineRegistration_ShouldReturnOKResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var httpContent = new { ciIdentifier = "CI1234567", environment = "1" };
        var jsonContent = JsonConvert.SerializeObject(httpContent);
        request.Content = new StringContent(jsonContent);
        var user = _fixture.Create<User>();

        _azdoClientMock
            .Setup(azdoRestClient => azdoRestClient.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _authorizationServiceMock
            .Setup(authorizationService =>
                authorizationService.HasEditPermissionsAsync(request, organization, project.Id, pipelineId,
                    pipelineType))
            .ReturnsAsync(true);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user);

        _registratorMock
            .Setup(pipelineRegistrator => pipelineRegistrator.RegisterProdPipelineAsync(organization, project.Id,
                pipelineId, pipelineType,
                It.IsAny<string>(), It.IsAny<RegistrationRequest>()))
            .ReturnsAsync(new OkResult())
            .Verifiable();

        _compliancyReportServiceMock
            .Setup(compliancyReportService => compliancyReportService.UpdateRegistrationAsync(organization,
                project.Name,
                pipelineId, pipelineType, It.IsAny<string>()))
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, project.Id, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkResult));
        _registratorMock.Verify(pipelineRegistrator => pipelineRegistrator.RegisterNonProdPipelineAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _registratorMock.Verify();
        _compliancyReportServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync_WhenCiIdentifierIsNull_ShouldReturnOkResult()
    {
        // Arrange
        var request = new HttpRequestMessage { Content = null };
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var user = _fixture.Create<User>();
        var httpContent = new { };
        var jsonContent = JsonConvert.SerializeObject(httpContent);
        request.Content = new StringContent(jsonContent);

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _authorizationServiceMock
            .Setup(x => x.HasEditPermissionsAsync(request, organization, project.Id, pipelineId, pipelineType))
            .ReturnsAsync(true);

        _authorizationServiceMock
            .Setup(x => x.GetInteractiveUserAsync(It.IsAny<HttpRequestMessage>(), It.IsAny<string>()))
            .ReturnsAsync(user);

        _registratorMock
            .Setup(x => x.RegisterNonProdPipelineAsync(organization, project.Id, pipelineId, pipelineType,
                It.IsAny<string>()))
            .ReturnsAsync(new OkResult())
            .Verifiable();

        _compliancyReportServiceMock
            .Setup(x => x.UpdateRegistrationAsync(organization, project.Name, pipelineId, pipelineType,
                It.IsAny<string>()))
            .Verifiable();

        // Act
        var actual = await _sut.RunAsync(request, organization, project.Id, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task RunAsync_WhenHttpContentIsNull_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var request = new HttpRequestMessage { Content = new StringContent(string.Empty) };
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var user = _fixture.Create<User>();
        var httpContent = new { };
        var jsonContent = JsonConvert.SerializeObject(httpContent);
        request.Content = new StringContent(jsonContent);

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _authorizationServiceMock
            .Setup(x => x.HasEditPermissionsAsync(request, organization, project.Id, pipelineId, pipelineType))
            .ReturnsAsync(true);

        _authorizationServiceMock.Setup(x => x.GetInteractiveUserAsync(It.IsAny<HttpRequestMessage>()
            , It.IsAny<string>())).ReturnsAsync(user);

        _registratorMock
            .Setup(x => x.RegisterNonProdPipelineAsync(organization, project.Id, pipelineId, pipelineType,
                It.IsAny<string>())).ThrowsAsync(new ArgumentNullException(It.IsAny<string>())).Verifiable();

        _compliancyReportServiceMock
            .Setup(x => x.UpdateRegistrationAsync(organization, project.Name,
                pipelineId, pipelineType, It.IsAny<string>())).Verifiable();

        // Act
        var actual = await _sut.RunAsync(request, organization, project.Id, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var request = new HttpRequestMessage { Content = new StringContent(string.Empty) };
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var httpContent = new { };
        var jsonContent = JsonConvert.SerializeObject(httpContent);
        request.Content = new StringContent(jsonContent);

        _validateInputServiceMock.Setup(m => m.Validate(request, organization, project.Id, pipelineId))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.RunAsync(request, organization, project.Id, pipelineId, pipelineType);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
                LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Once);
    }
}