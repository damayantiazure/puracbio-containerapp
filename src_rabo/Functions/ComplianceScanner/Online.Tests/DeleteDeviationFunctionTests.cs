#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Storage;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.Deviations;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests.Helpers;
using Rabobank.Compliancy.Functions.Shared.Tests;
using System;
using System.Net;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class DeleteDeviationFunctionTests : FunctionRequestTests
{
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcess;
    private readonly Mock<IDeleteDeviationProcess> _deleteDeviationProcess;
    private readonly IFixture _fixture = new Fixture();
    private readonly DeleteDeviationFunction _sut;
    private readonly string _token;
    private readonly Mock<ILoggingService> _loggingServiceMock;
    private readonly Mock<ISecurityContext> _securityContextMock;

    public DeleteDeviationFunctionTests()
    {
        _checkAuthorizationProcess = new Mock<ICheckAuthorizationProcess>();
        _deleteDeviationProcess = new Mock<IDeleteDeviationProcess>();
        _loggingServiceMock = new Mock<ILoggingService>();
        _securityContextMock = new Mock<ISecurityContext>();

        _token = _fixture.Create<string>();
        _sut = new DeleteDeviationFunction(_checkAuthorizationProcess.Object, _deleteDeviationProcess.Object,
            Mock.Of<IHttpContextAccessor>(), _loggingServiceMock.Object, _securityContextMock.Object);
    }

    [Fact]
    public async Task RunAsync_WithUnauthorizedUser_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                default)).ReturnsAsync(false).Verifiable();

        // Act
        var actual = await _sut.RunAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<UnauthorizedResult>();
        _checkAuthorizationProcess.Verify();
    }

    [Fact]
    public async Task RunAsync_WithAuthorizedUser_ShouldDeleteDeviationRecord()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                default)).ReturnsAsync(true).Verifiable();

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Verifiable();

        // Act
        var actual = await _sut.RunAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<OkResult>();
        _checkAuthorizationProcess.Verify();
        _deleteDeviationProcess.Verify();
    }

    [Fact]
    public async Task RunAsync_WhenProcessThrowsStorageExceptionWithStatusCode401_ShouldReturnOkResult()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();
        var requestResult = _fixture.Build<RequestResult>()
            .With(result => result.HttpStatusCode, (int)HttpStatusCode.NotFound)
            .Create();

        var exceptionMessage = _fixture.Create<string>();
        var innerException = _fixture.Create<Exception>();
        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                default)).ReturnsAsync(true).Verifiable();

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Throws(new StorageException(requestResult, exceptionMessage, innerException))
            .Verifiable();

        // Act
        var actual = await _sut.RunAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<OkResult>();
        _checkAuthorizationProcess.Verify();
        _deleteDeviationProcess.Verify();
        _loggingServiceMock.Verify(
            validateInputService =>
                validateInputService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()),
            Times.Never);
    }

    [Fact]
    public async Task RunAsync_WhenProcessThrowsException_ShouldThrowException()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();
        var exception = _fixture.Create<Exception>();

        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                default)).ReturnsAsync(true);

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Throws(exception);

        _loggingServiceMock.Setup(validateInputService =>
            validateInputService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()));

        // Act
        var actual = () => _sut.RunAsync(deleteDeviationRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(validateInputService => validateInputService.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(exceptionBaseMetaInformation =>
                exceptionBaseMetaInformation.Function == nameof(DeleteDeviationFunction) &&
                exceptionBaseMetaInformation.Organization == deleteDeviationRequest.Organization &&
                exceptionBaseMetaInformation.ProjectId == deleteDeviationRequest.ProjectId.ToString()
            ), exception, deleteDeviationRequest.ItemId, deleteDeviationRequest.RuleName,
            deleteDeviationRequest.CiIdentifier));
    }

    [Fact]
    public async Task DeleteDeviationAsync_WithUnauthorizedUser_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                default)).ReturnsAsync(false).Verifiable();

        // Act
        var actual = await _sut.DeleteDeviationAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<UnauthorizedResult>();
        _checkAuthorizationProcess.Verify();
    }

    [Fact]
    public async Task DeleteDeviationAsync_WithAuthorizedUser_ShouldDeleteDeviationRecord()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), default))
            .ReturnsAsync(true).Verifiable();

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Verifiable();

        // Act
        var actual = await _sut.DeleteDeviationAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<OkResult>();
        _checkAuthorizationProcess.Verify();
        _deleteDeviationProcess.Verify();
    }

    [Fact]
    public async Task DeleteDeviationAsync_WhenProcessThrowsStorageExceptionWithStatusCode401_ShouldReturnOkResult()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();
        var requestResult = _fixture.Build<RequestResult>()
            .With(result => result.HttpStatusCode, (int)HttpStatusCode.NotFound)
            .Create();

        var exceptionMessage = _fixture.Create<string>();
        var innerException = _fixture.Create<Exception>();
        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), default)).ReturnsAsync(true);

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Throws(new StorageException(requestResult, exceptionMessage, innerException));

        // Act
        var actual = await _sut.DeleteDeviationAsync(deleteDeviationRequest, httpRequest);

        // Assert
        actual.Should().BeOfType<OkResult>();
        _checkAuthorizationProcess.Verify();
        _deleteDeviationProcess.Verify();
        _loggingServiceMock.Verify(
            validateInputService =>
                validateInputService.LogExceptionAsync(It.IsAny<LogDestinations>(), It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<Exception>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteDeviationAsync_WhenProcessThrowsException_ShouldThrowException()
    {
        // Arrange
        var httpRequest = HttpRequestHelpers.CreateHttpRequestMock(_token);
        var deleteDeviationRequest = _fixture.Create<DeleteDeviationRequest>();

        var exception = _fixture.Create<Exception>();
        _checkAuthorizationProcess
            .Setup(checkAuthorizationProcess => checkAuthorizationProcess.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), default)).ReturnsAsync(true);

        _deleteDeviationProcess.Setup(deleteDeviationProcess =>
                deleteDeviationProcess.DeleteDeviationAsync(deleteDeviationRequest, default))
            .Throws(exception);

        _loggingServiceMock.Setup(validateInputService => validateInputService.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()));

        // Act
        var actual = () => _sut.DeleteDeviationAsync(deleteDeviationRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(validateInputService => validateInputService.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.Is<ExceptionBaseMetaInformation>(exceptionBaseMetaInformation =>
                exceptionBaseMetaInformation.Function == nameof(_sut.DeleteDeviationAsync) &&
                exceptionBaseMetaInformation.Organization == deleteDeviationRequest.Organization &&
                exceptionBaseMetaInformation.ProjectId == deleteDeviationRequest.ProjectId.ToString()
            ), exception, deleteDeviationRequest.ItemId, deleteDeviationRequest.RuleName,
            deleteDeviationRequest.CiIdentifier));
    }
}