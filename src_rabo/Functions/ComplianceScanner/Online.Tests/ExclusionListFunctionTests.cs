#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Shouldly;
using System;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests;

public class ExclusionListFunctionTests : FunctionTestBase
{
    private readonly Mock<ICheckAuthorizationProcess> _checkAuthorizationProcessMock = new();
    private readonly Mock<IExclusionListProcess> _exclusionListProcessMock = new();
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock = new();
    private readonly ExclusionListFunction _sut;
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<ISecurityContext> _securityContextMock = new();

    public ExclusionListFunctionTests() =>
        _sut = new ExclusionListFunction(_exclusionListProcessMock.Object, _checkAuthorizationProcessMock.Object,
            _loggingServiceMock.Object, _httpContextAccessorMock.Object, _securityContextMock.Object);

    [Fact]
    public async Task GetCreateOrUpdateExclusionListAsync_WithNoUserEditPermission_ShouldReturnUnAuthorizedResult()
    {
        // Arrange
        var httpRequestMock = CreateHttpRequestMock();
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userPermission = _fixture.Build<UserPermission>().With(x => x.IsAllowedToEditPermissions, false).Create();

        _checkAuthorizationProcessMock.Setup(x =>
                x.GetUserPermissionAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                    default))
            .ReturnsAsync(userPermission).Verifiable();

        // Act
        var actual = await _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, httpRequestMock);

        // Assert
        actual.ShouldBeOfType(typeof(UnauthorizedResult));
    }

    [Fact]
    public async Task CreateOrUpdateExclusionListAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var httpRequestMock = CreateHttpRequestMock();
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();

        _checkAuthorizationProcessMock.Setup(x =>
                x.GetUserPermissionAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                    default))
            .Throws<InvalidOperationException>();

        // Act
        var actual = () => _sut.CreateOrUpdateExclusionListAsync(exclusionListRequest, httpRequestMock);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(x =>
            x.LogExceptionAsync(
                LogDestinations.ComplianceScannerOnlineErrorLog,
                It.IsAny<InvalidOperationException>(),
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            ), Times.Once);
    }

    [Fact]
    public async Task PostCreateOrUpdateExclusionListAsync_WithNoUserEditPermission_ShouldReturnUnAuthorizedResult()
    {
        // Arrange
        var httpRequestMock = CreateHttpRequestMock();
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userPermission = _fixture.Build<UserPermission>().With(x => x.IsAllowedToEditPermissions, false).Create();

        _checkAuthorizationProcessMock.Setup(x =>
                x.GetUserPermissionAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                    default))
            .ReturnsAsync(userPermission).Verifiable();

        // Act
        var actual = await _sut.PostCreateOrUpdateExclusionListAsync(exclusionListRequest, httpRequestMock);

        // Assert
        actual.ShouldBeOfType(typeof(UnauthorizedResult));
    }

    [Fact]
    public async Task PostCreateOrUpdateExclusionListAsync_WithUserEditPermission_ShouldReturnOkObjectResult()
    {
        // Arrange
        var httpRequestMock = CreateHttpRequestMock();
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userPermission = _fixture.Build<UserPermission>().With(x => x.IsAllowedToEditPermissions, true).Create();

        _checkAuthorizationProcessMock.Setup(x =>
                x.GetUserPermissionAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                    default))
            .ReturnsAsync(userPermission).Verifiable();

        _exclusionListProcessMock.Setup(x =>
                x.CreateOrUpdateExclusionListAsync(exclusionListRequest, userPermission.User, default))
            .Verifiable();

        // Act
        var actual = await _sut.PostCreateOrUpdateExclusionListAsync(exclusionListRequest, httpRequestMock);

        // Assert
        actual.ShouldBeOfType(typeof(OkObjectResult));
        _exclusionListProcessMock.Verify();
    }

    [Fact]
    public async Task
        PostCreateOrUpdateExclusionListAsync_WhenInvalidApproverExceptionIsThrown_ShouldReturnBadRequestObjectResult()
    {
        // Arrange
        var httpRequestMock = CreateHttpRequestMock();
        var exclusionListRequest = _fixture.Create<ExclusionListRequest>();
        var userPermission = _fixture.Build<UserPermission>().With(x => x.IsAllowedToEditPermissions, true).Create();

        _checkAuthorizationProcessMock.Setup(x =>
                x.GetUserPermissionAsync(It.IsAny<AuthorizationRequest>(), It.IsAny<AuthenticationHeaderValue>(),
                    default))
            .ReturnsAsync(userPermission).Verifiable();

        _exclusionListProcessMock.Setup(x =>
                x.CreateOrUpdateExclusionListAsync(exclusionListRequest, userPermission.User, default))
            .Throws<InvalidExclusionRequesterException>();

        _loggingServiceMock.Setup(x => x.LogExceptionAsync(
            LogDestinations.ComplianceScannerOnlineErrorLog,
            It.IsAny<Exception>(),
            It.IsAny<ExceptionBaseMetaInformation>(), exclusionListRequest.PipelineId.ToString(), exclusionListRequest.PipelineType))
            .Verifiable();

        // Act
        var actual = await _sut.PostCreateOrUpdateExclusionListAsync(exclusionListRequest, httpRequestMock);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
        _loggingServiceMock.Verify();
    }
}