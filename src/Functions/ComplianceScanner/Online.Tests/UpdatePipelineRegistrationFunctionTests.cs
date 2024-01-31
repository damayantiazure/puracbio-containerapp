#nullable enable

using System;
using System.Net.Http;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;
using Task = System.Threading.Tasks.Task;
using User = Rabobank.Compliancy.Domain.Compliancy.Authorizations.User;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class UpdatePipelineRegistrationFunctionTests : FunctionRequestTests
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly Mock<ICompliancyReportService> _compliancyReportServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IPipelineRegistrator> _registratorMock = new();
    private readonly UpdatePipelineRegistrationFunction _sut;
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();

    public UpdatePipelineRegistrationFunctionTests() =>
        _sut = new UpdatePipelineRegistrationFunction(_azdoClientMock.Object, _registratorMock.Object,
            _validateInputServiceMock.Object, _authorizationServiceMock.Object, _checkAuthorizationProcessMock.Object,
            _compliancyReportServiceMock.Object, _loggingServiceMock.Object);

    [Fact]
    public async Task RunAsync_WithInvalidProfile_ShouldHaveIsInvalidValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .With(x => x.FieldToUpdate, FieldToUpdate.CiIdentifier.ToString())
            .With(x => x.Profile, _fixture.Create<string>())
            .With(x => x.NewValue, _fixture.Create<string>()).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType)
            as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsInvalidText, nameof(updateRequest.Profile)));
    }

    [Fact]
    public async Task RunAsync_WithInvalidProfileNewValue_ShouldHaveIsInvalidValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .With(x => x.FieldToUpdate, FieldToUpdate.Profile.ToString())
            .With(x => x.Profile, Profiles.Default.ToString())
            .With(x => x.NewValue, _fixture.Create<string>()).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType)
            as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsInvalidText, nameof(updateRequest.NewValue)));
    }

    [Fact]
    public async Task RunAsync_WithEmptyNewValue_ShouldHaveIsRequiredValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .With(x => x.FieldToUpdate, _fixture.Create<string>())
            .With(x => x.Profile, Profiles.Default.ToString())
            .Without(x => x.NewValue).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType)
            as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsRequiredText, nameof(updateRequest.NewValue)));
    }

    [Fact]
    public async Task RunAsync_WithEmptyFieldToUpdate_ShouldHaveIsRequiredValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .Without(x => x.FieldToUpdate)
            .With(x => x.Profile, Profiles.Default.ToString())
            .With(x => x.NewValue, _fixture.Create<string>()).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType)
            as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsRequiredText, nameof(updateRequest.FieldToUpdate)));
    }

    [Fact]
    public async Task RunAsync_WithInvalidFieldToUpdate_ShouldHaveIsInvalidValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .Without(x => x.FieldToUpdate)
            .With(x => x.Profile, Profiles.Default.ToString())
            .With(x => x.NewValue, _fixture.Create<string>()).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType)
            as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsInvalidText, nameof(updateRequest.FieldToUpdate)));
    }

    [Fact]
    public async Task RunAsync_WithEmptyEnvironment_ShouldHaveIsRequiredValidationError()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .Without(x => x.Environment).Create();

        var request = CreateHttpRequest(updateRequest);

        // Act
        var actual =
            await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType) as BadRequestObjectResult;

        // Assert
        actual.ShouldNotBeNull();
        actual.Value!.ToString().Should()
            .Contain(string.Format(Constants.IsRequiredText, nameof(updateRequest.Environment)));
    }

    [Fact]
    public async Task RunAsync_WithEmptyCiIdentifier_ShouldNotRunUpdateProdPipelineRegistrationAsync()
    {
        // Arrange
        var authorizationRequest = new AuthorizationRequest(_fixture.Create<Guid>(), _fixture.Create<string>());
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Build<UpdateRequest>()
            .Without(x => x.CiIdentifier).Create();

        var request = CreateHttpRequest(updateRequest);

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(authorizationRequest, default, default))
            .ReturnsAsync(true);

        _registratorMock
            .Setup(x => x.UpdateProdPipelineRegistrationAsync(organization, projectId.ToString(), pipelineId,
                pipelineType, It.IsAny<string>(), It.IsAny<UpdateRequest>()))
            .ReturnsAsync(new OkResult()).Verifiable();

        // Act
        await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        _registratorMock.Verify(x => x.UpdateProdPipelineRegistrationAsync(organization, projectId.ToString(),
            pipelineId, pipelineType,
            It.IsAny<string>(), It.IsAny<UpdateRequest>()), Times.Never());
    }

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(x => x.Validate(TestRequest, organization, projectId.ToString(), pipelineId))
            .Throws(exception)
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(TestRequest, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
            exception, It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(UpdatePipelineRegistrationFunction) &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId.ToString()), pipelineId, It.IsAny<string>())
        );
    }

    [Fact]
    public async Task UnauthorizedUser_ShouldReturn_UnauthorizedResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;

        var updateRequest = _fixture.Build<UpdateRequest>()
            .With(x => x.FieldToUpdate, FieldToUpdate.CiIdentifier.ToString())
            .With(x => x.Profile, Profiles.Default.ToString()).Create();
        var request = CreateHttpRequest(updateRequest);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.HasEditPermissionsAsync(
                request, organization, projectId.ToString(), pipelineId, pipelineType))
            .ReturnsAsync(false)
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(UnauthorizedResult));
    }

    [Fact]
    public async Task RunAsync_UpdatePipelineRegistration_ShouldReturnOKResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _fixture.Customize<Project>(c => c.With(p => p.Id, projectId.ToString()));
        var user = _fixture.Create<User>();
        var organization = _fixture.Create<string>();
        var project = _fixture.Create<Project>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        var updateRequest = new UpdateRequest
        {
            FieldToUpdate = FieldToUpdate.CiIdentifier.ToString(),
            CiIdentifier = "CI1234567",
            Environment = "1",
            Profile = Profiles.Default.ToString(),
            NewValue = "CI8912345"
        };

        var request = CreateHttpRequest(updateRequest);

        _azdoClientMock
            .Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Project>>(), organization))
            .ReturnsAsync(project)
            .Verifiable();

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(), default, default))
            .ReturnsAsync(true);

        _authorizationServiceMock
            .Setup(x => x.GetInteractiveUserAsync(request, organization))
            .ReturnsAsync(user);

        _registratorMock
            .Setup(x => x.UpdateProdPipelineRegistrationAsync(organization, project.Id, pipelineId, pipelineType,
                It.IsAny<string>(), It.IsAny<UpdateRequest>()))
            .ReturnsAsync(new OkObjectResult(It.IsAny<string>())).Verifiable();

        _compliancyReportServiceMock
            .Setup(x => x.UpdateRegistrationAsync(organization, project.Name, pipelineId, pipelineType,
                It.IsAny<string>()))
            .Verifiable();

        // Act
        var result = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        result.ShouldBeOfType(typeof(OkObjectResult));
        _registratorMock.Verify();
        _compliancyReportServiceMock.Verify();
    }

    [Fact]
    public async Task RunAsync_WhenHttpContentIsNull_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var projectId = Guid.NewGuid();
        _fixture.Customize<Project>(c => c.With(p => p.Id, projectId.ToString()));
        var authorizationRequest = new AuthorizationRequest(_fixture.Create<Guid>(), _fixture.Create<string>());
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

        _checkAuthorizationProcessMock
            .Setup(x => x.IsAuthorized(authorizationRequest, default, default))
            .ReturnsAsync(true);

        _authorizationServiceMock.Setup(x => x.GetInteractiveUserAsync(It.IsAny<HttpRequestMessage>()
            , It.IsAny<string>())).ReturnsAsync(user);

        _registratorMock
            .Setup(x => x.UpdateProdPipelineRegistrationAsync(organization, project.Id, pipelineId, pipelineType,
                user.MailAddress, It.IsAny<UpdateRequest>()))
            .ThrowsAsync(new ArgumentNullException(_fixture.Create<string>()));

        _compliancyReportServiceMock
            .Setup(x => x.UpdateRegistrationAsync(organization, project.Name,
                pipelineId, pipelineType, It.IsAny<string>())).Verifiable();

        // Act
        var actual = await _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var updateRequest = _fixture.Create<UpdateRequest>();
        var request = CreateHttpRequest(updateRequest);

        _validateInputServiceMock.Setup(m => m.Validate(request, organization, projectId.ToString(), pipelineId))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<InvalidOperationException>(),
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<string>(), It.IsAny<string>()));
    }

    private static HttpRequestMessage CreateHttpRequest(UpdateRequest updateRequest)
        => new()
        {
            Content = new StringContent(JsonConvert.SerializeObject(updateRequest),
                Encoding.UTF8, "application/json")
        };
}