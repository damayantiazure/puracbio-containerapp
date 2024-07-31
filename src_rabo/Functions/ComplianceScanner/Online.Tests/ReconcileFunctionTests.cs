#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Interfaces.Reconcile;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Shouldly;
using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class ReconcileFunctionTests : FunctionRequestTests
{
    private static readonly IFixture Fixture = new Fixture();

    private static readonly string Token = Fixture.Create<string>();
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly Mock<IItemReconcileProcess> _itemReconcileProcessMock = new();
    private readonly Mock<IProjectReconcileProcess> _projectReconcileProcessMock = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();
    private readonly Mock<IReconcileProcess> _reconcileProcessMock = new();
    private readonly ReconcileFunction _sut;
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    public ReconcileFunctionTests()
    {
        _sut = new ReconcileFunction(_reconcileProcessMock.Object, _itemReconcileProcessMock.Object,
            _projectReconcileProcessMock.Object, _loggingServiceMock.Object, _checkAuthorizationProcessMock.Object,
            _httpContextAccessorMock.Object, _securityContextMock.Object);

        Fixture.Customize<ReconcileRequest>(composer =>
            composer.With(x => x.RuleName, RuleNames.NobodyCanDeleteBuilds));
    }

    [Fact]
    public async Task RunAsync_WithInvalidRuleName_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _reconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(false);

        //  Act
        var actual = await _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ItemReconcileAsync_WithInvalidRuleName_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _itemReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(false);

        //  Act
        var actual = await _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProjectReconcileAsync_WithInvalidRuleName_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _projectReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(false);

        //  Act
        var actual = await _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WithUnAuthorizedToken_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _reconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        //  Act
        var actual = await _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task ItemReconcileAsync_WithUnAuthorizedToken_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _itemReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        //  Act
        var actual = await _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task ProjectReconcileAsync_WithUnAuthorizedToken_ShouldReturnUnauthorizedResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _projectReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        //  Act
        var actual = await _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task RunAsync_WithAuthorizedToken_ShouldReturnOkResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _reconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        //  Act
        var actual = await _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<OkResult>();
        _reconcileProcessMock.Verify(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ItemReconcileAsync_WithAuthorizedToken_ShouldReturnOkResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _itemReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        //  Act
        var actual = await _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<OkResult>();
        _itemReconcileProcessMock.Verify(
            x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProjectReconcileAsync_WithAuthorizedToken_ShouldReturnOkResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _projectReconcileProcessMock.Setup(x => x.HasRuleName(It.IsAny<string>())).Returns(true);

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        //  Act
        var actual = await _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<OkResult>();
        _projectReconcileProcessMock.Verify(
            x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_WhenEnvironmentNotFoundExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _reconcileProcessMock.Setup(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EnvironmentNotFoundException());

        //  Act
        var actual = await _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ItemReconcileAsync_WhenEnvironmentNotFoundExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _itemReconcileProcessMock
            .Setup(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EnvironmentNotFoundException());

        //  Act
        var actual = await _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProjectReconcileAsync_WhenEnvironmentNotFoundExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        _checkAuthorizationProcessMock.Setup(x => x.IsAuthorized(It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        _projectReconcileProcessMock
            .Setup(x => x.ReconcileAsync(It.IsAny<ReconcileRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EnvironmentNotFoundException());

        //  Act
        var actual = await _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WhenArgumentExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        //  Act
        var actual = await _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task RunAsync_WhenExceptionIsThrown_ShouldThrowException()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();
        _reconcileProcessMock
            .Setup(x => x.HasRuleName(It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.RunAsync(reconcileRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<InvalidOperationException>()), Times.Once);
    }

    [Fact]
    public async Task ItemReconcileAsync_WhenArgumentExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        //  Act
        var actual = await _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ItemReconcileAsync_WhenExceptionIsThrown_ShouldThrowException()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();
        _itemReconcileProcessMock
            .Setup(x => x.HasRuleName(It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.ItemReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<InvalidOperationException>()), Times.Once);
    }

    [Fact]
    public async Task ProjectReconcileAsync_WhenArgumentExceptionIsThrown_ShouldReturnBadObjectResult()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();

        //  Act
        var actual = await _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProjectReconcileAsync_WhenExceptionIsThrown_ShouldThrowException()
    {
        // Arrange
        var httpRequest = CreateHttpRequest();
        var reconcileRequest = Fixture.Create<ReconcileRequest>();
        _projectReconcileProcessMock
            .Setup(x => x.HasRuleName(It.IsAny<string>()))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.ProjectReconcileAsync(reconcileRequest, httpRequest);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(item => item.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<InvalidOperationException>()), Times.Once);
    }

    private static HttpRequest CreateHttpRequest()
    {
        var httpRequest = new Mock<HttpRequest>();

        IHeaderDictionary headers = new HeaderDictionary { { "Authorization", Token } };
        httpRequest.SetupGet(x => x.Headers)
            .Returns(headers);

        return httpRequest.Object;
    }
}