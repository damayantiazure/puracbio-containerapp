#nullable enable

using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Shouldly;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class IncludeNonProdFunctionTests : FunctionRequestTests
{
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IPipelineRegistrator> _registratorMock = new();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;
        var exception = _fixture.Create<ArgumentNullException>();

        _validateInputServiceMock
            .Setup(validateInputService =>
                validateInputService.Validate(organization, projectId.ToString(), pipelineId, TestRequest))
            .Throws(exception)
            .Verifiable();

        var sut = new IncludeNonProdFunction(_registratorMock.Object, _validateInputServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(TestRequest, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(e =>
                e.Function == nameof(IncludeNonProdFunction) &&
                e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                e.Organization == organization &&
                e.ProjectId == projectId.ToString()), exception, pipelineId,
            It.IsAny<string>(), It.IsAny<string>())
        );
    }

    [Fact]
    public async Task UnauthorizedUser_ShouldReturn_UnauthorizedResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _checkAuthorizationProcessMock
            .Setup(checkAuthorizationProcess =>
                checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(), default, default))
            .ReturnsAsync(false)
            .Verifiable();

        var sut = new IncludeNonProdFunction(_registratorMock.Object, _validateInputServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(UnauthorizedResult));
        _checkAuthorizationProcessMock.Verify();
    }

    [Fact]
    public async Task WhenAllRequirementsAreMet_EmptyRequestBody_ShouldReturn_OKResult()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _checkAuthorizationProcessMock
            .Setup(checkAuthorizationProcess =>
                checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(), default, default))
            .ReturnsAsync(true);

        _registratorMock
            .Setup(pipelineRegistrator => pipelineRegistrator.UpdateNonProdRegistrationAsync(organization,
                projectId.ToString(), pipelineId, pipelineType,
                null))
            .ReturnsAsync(new OkResult())
            .Verifiable();

        var sut = new IncludeNonProdFunction(_registratorMock.Object, _validateInputServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(OkResult));
        _registratorMock.Verify();
    }

    [Theory]
    [InlineData("MyAwesomeStage")]
    [InlineData(null)]
    public async Task StageIdFromBodyBeingEmptyOrFilled_ShouldReturn_OKResult(string stageId)
    {
        // Arrange
        var request = new HttpRequestMessage
        {
            Content = new StringContent($"{{ \"environment\": \"{stageId}\" }}")
        };
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _checkAuthorizationProcessMock
            .Setup(checkAuthorizationProcess =>
                checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(), default, default))
            .ReturnsAsync(true);

        _registratorMock
            .Setup(pipelineRegistrator => pipelineRegistrator.UpdateNonProdRegistrationAsync(organization,
                projectId.ToString(), pipelineId, pipelineType,
                stageId))
            .ReturnsAsync(new OkResult())
            .Verifiable();

        var sut = new IncludeNonProdFunction(_registratorMock.Object, _validateInputServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        actual.ShouldBeOfType(typeof(OkResult));
        _registratorMock.Verify();
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var pipelineId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.YamlReleasePipeline;

        _checkAuthorizationProcessMock.Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(
                It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .Throws<InvalidOperationException>();

        var sut = new IncludeNonProdFunction(_registratorMock.Object, _validateInputServiceMock.Object,
            _checkAuthorizationProcessMock.Object, _loggingServiceMock.Object);

        // Act
        var actual = () => sut.RunAsync(request, organization, projectId, pipelineId, pipelineType);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(loggingService =>
            loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
    }
}