#nullable enable

using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;
using Task = System.Threading.Tasks.Task;
using User = Rabobank.Compliancy.Domain.Compliancy.Authorizations.User;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class DeletePipelineRegistrationFunctionTests
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    private readonly string _organization;
    private readonly string _pipelineId;

    private readonly Mock<IPipelineRegistrator> _pipelineRegistratorMock = new();
    private readonly string _pipelineType;
    private readonly Guid _projectId;

    private readonly DeletePipelineRegistrationFunction _sut;
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();

    public DeletePipelineRegistrationFunctionTests()
    {
        _organization = _fixture.Create<string>();
        _projectId = _fixture.Create<Guid>();
        _pipelineId = _fixture.Create<string>();
        _pipelineType = _fixture.Create<string>();

        _sut = new DeletePipelineRegistrationFunction(_pipelineRegistratorMock.Object, _authorizationServiceMock.Object,
            _validateInputServiceMock.Object, _azdoClientMock.Object, _compliancyReportServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        InitializeValidationSetup();
    }

    [Fact]
    public async Task RunAsync_WithNoCiIdentifier_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var registrationRequest = _fixture.Build<RegistrationRequest>().Without(x => x.CiIdentifier).Create();

        var requestMessage = CreateHttpRequest(registrationRequest);

        // Act
        var actual = await _sut.RunAsync(requestMessage, _organization, _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WithNoEnvironment_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var registrationRequest = _fixture.Build<RegistrationRequest>().Without(x => x.Environment).Create();

        var requestMessage = CreateHttpRequest(registrationRequest);

        // Act
        var actual = await _sut.RunAsync(requestMessage, _organization, _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WithOrganization_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var requestMessage = CreateHttpRequest(new { environment = "Test" });

        // Act
        var actual = await _sut.RunAsync(requestMessage, null, _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WithUnauthorizedUser_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var authorizationRequest = new AuthorizationRequest(_fixture.Create<Guid>(), _fixture.Create<string>());
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var request = CreateHttpRequest(registrationRequest);

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(authorizationRequest, default, default))
            .ReturnsAsync(false);

        // Act
        var actual = await _sut.RunAsync(request, _organization
            , _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task RunAsync_WithInvalidRequestContent_ShouldReturnBadRequestResult()
    {
        // Arrange
        var request = CreateHttpRequest(null);
        var authorizationRequest = new AuthorizationRequest(_fixture.Create<Guid>(), _fixture.Create<string>());

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(authorizationRequest, default, default))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut.RunAsync(request, _organization
            , _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task RunAsync_WithInvalidRequestContentAsNull_ShouldReturnBadRequestResult()
    {
        // Arrange
        var authorizationRequest = new AuthorizationRequest(_fixture.Create<Guid>(), _fixture.Create<string>());
        var request = _fixture.Build<HttpRequestMessage>()
            .Without(x => x.Content).Create();

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(authorizationRequest, default, default))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut.RunAsync(request, _organization
            , _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task RunAsync_LogExceptionWhenExceptionIsThrown_ShouldThrowException()
    {
        // Arrange
        _validateInputServiceMock
            .Setup(m => m.Validate(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<string>()))
            .Throws<InvalidOperationException>();
        var request = CreateHttpRequest(new object());

        // Act
        var actual = () => _sut.RunAsync(request, _organization, _projectId, _pipelineId, _pipelineType);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>()
            , It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WithValidRequestInputContent_ShouldReturnOkResult()
    {
        // Arrange
        var registrationRequest = _fixture.Create<RegistrationRequest>();
        var request = CreateHttpRequest(registrationRequest);
        var user = _fixture.Create<User>();
        var okResult = _fixture.Create<OkObjectResult>();
        var project = _fixture.Build<Project>()
            .With(f => f.Id, _projectId.ToString)
            .Create();

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(), default, default))
            .ReturnsAsync(true);

        _authorizationServiceMock.Setup(x => x.GetInteractiveUserAsync(request, _organization))
            .ReturnsAsync(user);

        _pipelineRegistratorMock.Setup(x =>
                x.DeleteProdPipelineRegistrationAsync(_organization, _projectId.ToString(), _pipelineId, _pipelineType
                    , user.MailAddress, It.IsAny<DeleteRegistrationRequest>()))
            .ReturnsAsync(okResult);

        _azdoClientMock
            .Setup(m =>
                m.GetAsync(It.IsAny<IAzdoRequest<Project>>(),
                    _organization)).ReturnsAsync(project);

        _compliancyReportServiceMock.Setup(x => x.UnRegisteredPipelineAsync(_organization, project.Name,
            _pipelineId, _pipelineType, It.IsAny<CancellationToken>())).Verifiable();

        // Act
        var actual = await _sut.RunAsync(request, _organization, _projectId, _pipelineId, _pipelineType);

        // Assert
        actual.Should().BeOfType<OkObjectResult>();
        _compliancyReportServiceMock.Verify();
    }

    private void InitializeValidationSetup()
    {
        // validate if request is null
        _validateInputServiceMock.Setup(x => x.Validate(
                It.Is<HttpRequestMessage>(m => m == null), It.IsAny<string>()
                , It.IsAny<string>()))
            .Throws<ArgumentNullException>();

        // validate if organization is null
        _validateInputServiceMock.Setup(x => x.Validate(
                It.IsAny<HttpRequestMessage>(), It.Is<string>(m => string.IsNullOrWhiteSpace(m))
                , It.IsAny<string>()))
            .Throws<ArgumentNullException>();

        // validate if projectId is null
        _validateInputServiceMock.Setup(x => x.Validate(
                It.IsAny<HttpRequestMessage>(), It.IsAny<string>()
                , It.Is<string>(m => string.IsNullOrWhiteSpace(m))))
            .Throws<ArgumentNullException>();

        // validate if request, organization and projectId is null
        _validateInputServiceMock.Setup(x => x.Validate(
                It.Is<HttpRequestMessage>(m => m == null), It.Is<string>(m => string.IsNullOrWhiteSpace(m))
                , It.Is<string>(m => string.IsNullOrWhiteSpace(m))))
            .Throws<ArgumentNullException>();
    }

    private static HttpRequestMessage CreateHttpRequest(object? jsonObject)
        => new()
        {
            Content = new StringContent(JsonConvert.SerializeObject(jsonObject),
                Encoding.UTF8, "application/json")
        };
}