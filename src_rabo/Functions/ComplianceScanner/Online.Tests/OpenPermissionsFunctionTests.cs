#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests.Helpers;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class OpenPermissionsFunctionTests : FunctionRequestTests
{
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcess = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessor = new();

    private readonly Mock<IOpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline>>
        _openBuildDefinitionPipelinePermissionProcess = new();

    private readonly Mock<IOpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline>>
        _openReleasePermissionProcess = new();

    private readonly Mock<IOpenGitRepoPermissionsProcess> _openRepositoryPermissionProcess = new();
    private readonly OpenPermissionsFunction _sut;
    private readonly string _token;
    private readonly Mock<ILoggingService> _loggingService = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();

    public OpenPermissionsFunctionTests()
    {
        _token = _fixture.Create<string>();
        _sut = new OpenPermissionsFunction(_httpContextAccessor.Object, _loggingService.Object,
            _checkAuthorizationProcess.Object, _openRepositoryPermissionProcess.Object,
            _openBuildDefinitionPipelinePermissionProcess.Object, _openReleasePermissionProcess.Object, _securityContextMock.Object);
    }

    [Theory]
    [InlineData(typeof(OpenGitRepoPermissionsRequest))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>))]
    public async Task OpenRepositoryPermissionsAsync_WithUnauthorizedUser_ShouldReturnUnauthorizedResult(
        Type requestType)
    {
        // Arrange
        var request = CreateRequest(requestType);
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        SetupAuthorizationResult(false);
        var sut = GetSut(requestType);

        // Act
        var actual = await sut(request, httpRequest);

        // Assert
        actual.Should().BeOfType<UnauthorizedResult>();
        _checkAuthorizationProcess.Verify();
    }

    [Theory]
    [InlineData(typeof(OpenGitRepoPermissionsRequest))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>))]
    public async Task OpenPermissionsAsync_HappyFlow_ShouldOpenPermissions(Type requestType)
    {
        // Arrange
        var request = CreateRequest(requestType);
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var sut = GetSut(requestType);
        SetupAuthorizationResult();

        SetupOpenPermissionMockForPermissionType(requestType);

        // Act
        var actual = await sut(request, httpRequest);

        // Assert
        actual.Should().BeOfType<OkResult>();
        _checkAuthorizationProcess.Verify();
        VerifyOpenPermissionProcess(requestType);
    }

    [Theory]
    [InlineData(typeof(OpenGitRepoPermissionsRequest))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>))]
    public async Task
        OpenRepositoryPermissionsAsync_WhenProcessThrows_IsProductionException_ShouldReturnBadRequestObjectResult(
            Type requestType)
    {
        // Arrange
        var request = CreateRequest(requestType);
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var date = _fixture.Create<DateTime>();
        var ciName = _fixture.Create<string>();
        var runUrl = _fixture.Create<string>();

        SetupAuthorizationResult();
        var isProductionItemException =
            new IsProductionItemException(date.ToString(CultureInfo.InvariantCulture), ciName, runUrl);
        SetupOpenPermissionMockForPermissionType(requestType, isProductionItemException);
        var sut = GetSut(requestType);

        // Act
        var actual = await sut(request, httpRequest);

        // Assert
        actual.Should().BeOfType<BadRequestObjectResult>();
        actual.As<BadRequestObjectResult>().Value!.ToString().Should().Contain(isProductionItemException.Message);
        _checkAuthorizationProcess.Verify();
        VerifyOpenPermissionProcess(requestType);
    }

    [Theory]
    [InlineData(typeof(OpenGitRepoPermissionsRequest))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>))]
    public async Task
        OpenPermissionsAsync_WhenProcessThrows_SourceItemNotFoundException_ShouldReturnBadRequestObjectResult(
            Type requestType)
    {
        // Arrange
        var request = CreateRequest(requestType);
        var sut = GetSut(requestType);
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var message = _fixture.Create<string>();
        var sourceItemNotFoundException = new SourceItemNotFoundException(message);
        SetupOpenPermissionMockForPermissionType(requestType, sourceItemNotFoundException);
        SetupAuthorizationResult();

        // Act
        var actual = await sut(request, httpRequest);

        // Assert
        actual.Should().BeOfType<BadRequestObjectResult>();
        actual.As<BadRequestObjectResult>().Value!.ToString().Should().Contain(sourceItemNotFoundException.Message);
        _checkAuthorizationProcess.Verify();
        VerifyOpenPermissionProcess(requestType);
    }

    [Theory]
    [InlineData(typeof(OpenGitRepoPermissionsRequest), nameof(OpenPermissionsFunction.OpenRepositoryPermissionsAsync))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>), nameof(OpenPermissionsFunction.OpenBuildPermissionsAsync))]
    [InlineData(typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>), nameof(OpenPermissionsFunction.OpenReleasePermissionsAsync))]
    public async Task OpenPermissionsAsync_WhenProcessThrows_Exception_ShouldReturnBadRequestObjectResult(
        Type requestType, string functionName)
    {
        // Arrange
        var request = CreateRequest(requestType);
        var sut = GetSut(requestType);

        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        _loggingService.Setup(loggingService =>
            loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()));
        SetupAuthorizationResult();

        var exception = _fixture.Create<Exception>();
        SetupOpenPermissionMockForPermissionType(requestType, exception);

        // Act
        var actual = () => sut(request, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingService.Verify(loggingService => loggingService.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(exceptionBaseMetaInfo =>
                exceptionBaseMetaInfo.Organization == request.Organization &&
                exceptionBaseMetaInfo.ProjectId == request.ProjectId.ToString() &&
                exceptionBaseMetaInfo.Function == functionName
            ), exception));
    }

    private void SetupAuthorizationResult(bool result = true)
    {
        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(result).Verifiable();
    }

    private void SetupOpenPermissionMockForPermissionType(Type requestType, Exception? exception = null)
    {
        var mockSetups = new Dictionary<Type, Action>
        {
            {
                typeof(OpenGitRepoPermissionsRequest), () =>
                {
                    var setup = _openRepositoryPermissionProcess
                        .Setup(x => x.OpenPermissionAsync(It.IsAny<OpenGitRepoPermissionsRequest>(),
                            It.IsAny<CancellationToken>()));
                    if (exception != null)
                    {
                        setup.Throws(exception);
                    }

                    setup.Verifiable();
                }
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>), () =>
                {
                    var setup = _openBuildDefinitionPipelinePermissionProcess
                        .Setup(x => x.OpenPermissionAsync(
                            It.IsAny<OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>>(),
                            It.IsAny<CancellationToken>()));
                    if (exception != null)
                    {
                        setup.Throws(exception);
                    }

                    setup.Verifiable();
                }
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>), () =>
                {
                    var setup = _openReleasePermissionProcess
                        .Setup(x => x.OpenPermissionAsync(
                            It.IsAny<OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>>(),
                            It.IsAny<CancellationToken>()));
                    if (exception != null)
                    {
                        setup.Throws(exception);
                    }

                    setup.Verifiable();
                }
            }
        };

        if (mockSetups.TryGetValue(requestType, out var setupAction))
        {
            setupAction();
        }
        else
        {
            throw new ArgumentException($"Unsupported request type: {requestType}");
        }
    }

    private RequestBase CreateRequest(Type requestType)
    {
        return requestType switch
        {
            _ when requestType == typeof(OpenGitRepoPermissionsRequest) =>
                _fixture.Create<OpenGitRepoPermissionsRequest>(),
            _ when requestType.IsGenericType &&
                   requestType.GetGenericTypeDefinition() == typeof(OpenPipelinePermissionsRequest<>) =>
                (RequestBase)Activator.CreateInstance(requestType)!,
            _ =>
                throw new ArgumentException($"Unsupported request type: {requestType}")
        };
    }

    private Func<object, HttpRequest, Task<IActionResult>> GetSut(Type requestType)
    {
        var actions = new Dictionary<Type, Func<object, HttpRequest, Task<IActionResult>>>
        {
            {
                typeof(OpenGitRepoPermissionsRequest), (request, httpRequest)
                    => _sut.OpenRepositoryPermissionsAsync((OpenGitRepoPermissionsRequest)request, httpRequest)!
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>), (request, httpRequest)
                    => _sut.OpenBuildPermissionsAsync(
                        (OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>)request,
                        httpRequest)!
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>), (request, httpRequest)
                    => _sut.OpenReleasePermissionsAsync(
                        (OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>)request, httpRequest)!
            }
        };

        if (actions.TryGetValue(requestType, out var action))
        {
            return action;
        }

        throw new ArgumentException($"Unsupported request type: {requestType}");
    }

    private void VerifyOpenPermissionProcess(Type requestType)
    {
        var verificationActions = new Dictionary<Type, Action>
        {
            {
                typeof(OpenGitRepoPermissionsRequest), () => { _openRepositoryPermissionProcess.Verify(); }
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>),
                () => { _openBuildDefinitionPipelinePermissionProcess.Verify(); }
            },
            {
                typeof(OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>),
                () => { _openReleasePermissionProcess.Verify(); }
            }
        };

        if (verificationActions.TryGetValue(requestType, out var action))
        {
            action();
        }
        else
        {
            throw new ArgumentException($"Unsupported request type: {requestType}");
        }
    }
}