#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Shared.Tests;
using Shouldly;
using System;
using System.Net.Http.Headers;
using System.Threading;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class HasPermissionsFunctionTests : FunctionRequestTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly Mock<ILoggingService> _loggingService = new();
    private readonly HasPermissionFunction _hasPermissionsFunctionDefault;

    public HasPermissionsFunctionTests()
    {
        _hasPermissionsFunctionDefault = new HasPermissionFunction(
            _checkAuthorizationProcessMock.Object,
            _loggingService.Object);
        TestRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _fixture.Create<string>());
    }

    [Fact]
    public async Task Should_Return_OkResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = Guid.NewGuid();
        var request = TestRequest;

        // Act
        var result = await _hasPermissionsFunctionDefault.HasPermission(request, organization, projectId, default);

        // Assert
        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task Should_Check_IsAuthorized_WithCorrectParameters()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = Guid.NewGuid();
        var request = TestRequest;

        // Act
        await _hasPermissionsFunctionDefault.HasPermission(request, organization, projectId, default);

        // Assert
        _checkAuthorizationProcessMock.Verify(process => process.IsAuthorized(
            It.Is<AuthorizationRequest>(authorizationRequest => authorizationRequest.Organization == organization &&
                                                                authorizationRequest.ProjectId == projectId),
            It.Is<AuthenticationHeaderValue>(headerValue =>
                headerValue.Parameter == request.Headers.Authorization!.Parameter
                && headerValue.Scheme == request.Headers.Authorization.Scheme), default));
    }

    [Fact]
    public async Task InvalidInput_ShouldReturn_BadRequestObjectResult_WithAggregatedExceptionMessages()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = Guid.Empty;
        var request = TestRequest;

        // Act
        var result = await _hasPermissionsFunctionDefault.HasPermission(request, organization, projectId, default);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result).Value!.ToString();
        badRequestResult!.ShouldContain("Value cannot be null. (Parameter 'organization')");
        badRequestResult!.ShouldContain("Value cannot be null. (Parameter 'projectId')");
    }

    [Fact]
    public async Task InvalidInput_Should_LogExceptionAsync()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = Guid.Empty;

        // Act
        await _hasPermissionsFunctionDefault.HasPermission(TestRequest, organization, projectId, default);

        // Assert
        _loggingService.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.Is<ExceptionBaseMetaInformation>(e =>
                    e.Function == nameof(HasPermissionFunction) &&
                    e.RequestUrl == TestRequest.RequestUri!.AbsoluteUri &&
                    e.Organization == organization &&
                    e.ProjectId == projectId.ToString()
                ), It.IsAny<AggregateException>()
            )
        );
    }

    [Fact]
    public async Task HasPermission_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();

        _checkAuthorizationProcessMock.Setup(m => m.IsAuthorized(
                It.IsAny<AuthorizationRequest>(),
                It.IsAny<AuthenticationHeaderValue>(),
                It.IsAny<CancellationToken>()))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _hasPermissionsFunctionDefault.HasPermission(TestRequest, organization, projectId, default);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingService.Verify(x =>
            x.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<InvalidOperationException>()), Times.Once);
    }
}