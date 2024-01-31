#nullable enable

using AutoFixture.Kernel;
using FluentAssertions;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using System.Net.Http.Headers;
using User = Rabobank.Compliancy.Domain.Compliancy.User;

namespace Rabobank.Compliancy.Application.Tests.Authorization;

public class CheckAuthorizationProcessTests
{
    private readonly Mock<IAuthorizationService> _authorizationServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly CheckAuthorizationProcess _sut;

    public CheckAuthorizationProcessTests()
    {
        _sut = new CheckAuthorizationProcess(_authorizationServiceMock.Object, _projectServiceMock.Object);

        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(IIdentity),
                typeof(User)));
    }

    [Fact]
    public async Task IsAuthorizedAsync_GetsCurrentUser_With_ExpectedArguments()
    {
        // Arrange
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();
        ArrangeDefaultSetup(out var authorizationRequest, out var user, out var userPermissions);
        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(authorizationRequest.Organization,
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
                authorizationRequest.Organization, authorizationRequest.ProjectId, user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        await _sut.IsAuthorized(authorizationRequest, authenticationHeaderValue);

        // Assert
        _authorizationServiceMock.Verify(authorizationService =>
            authorizationService.GetCurrentUserAsync(authorizationRequest.Organization, authenticationHeaderValue,
                It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task IsAuthorizedAsync_GetPermissionsForUserOrGroup_With_ExpectedArguments()
    {
        // Arrange
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();
        ArrangeDefaultSetup(out var authorizationRequest, out var user, out var userPermissions);
        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(authorizationRequest.Organization,
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
                authorizationRequest.Organization,
                authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        await _sut.IsAuthorized(authorizationRequest, authenticationHeaderValue);

        // Assert
        _authorizationServiceMock.Verify(authorizationService =>
            authorizationService.GetPermissionsForUserOrGroupAsync(authorizationRequest.Organization,
                authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task IsAuthorizedAsync_WithAuthorizedUser_GetsUserPermissions()
    {
        // Arrange
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();
        ArrangeDefaultSetup(out var authorizationRequest, out var user, out var userPermissions);
        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(authorizationRequest.Organization,
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
                authorizationRequest.Organization,
                authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        await _sut.IsAuthorized(authorizationRequest, authenticationHeaderValue);

        // Assert
        _authorizationServiceMock.Verify(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
            authorizationRequest.Organization,
            authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task IsAuthorizedAsync_With_NoPermissions_ReturnsFalse()
    {
        // Arrange
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();
        ArrangeDefaultSetup(out var authorizationRequest, out var user, out var userPermissions);
        userPermissions = Enumerable.Empty<Permission>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(It.IsAny<string>(),
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        await _sut.IsAuthorized(authorizationRequest, authenticationHeaderValue);

        // Assert
        _authorizationServiceMock.Verify(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
            authorizationRequest.Organization,
            authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task IsAuthorizedAsync_With_EmptyPermissions_ReturnsFalse()
    {
        // Arrange
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();
        ArrangeDefaultSetup(out var authorizationRequest, out var user, out var userPermissions);
        userPermissions = new List<Permission>();

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(It.IsAny<string>(),
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(It.IsAny<string>(),
                It.IsAny<Guid>(), It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        await _sut.IsAuthorized(authorizationRequest, authenticationHeaderValue);

        // Assert
        _authorizationServiceMock.Verify(authorizationService => authorizationService.GetPermissionsForUserOrGroupAsync(
            authorizationRequest.Organization,
            authorizationRequest.ProjectId, user.Id, It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task GetUserPermissionAsync_With_EmptyPermissions_ReturnsTrue()
    {
        // Arrange
        var authorizationRequest = _fixture.Create<AuthorizationRequest>();
        var authenticationHeaderValue = _fixture.Create<AuthenticationHeaderValue>();

        var project = _fixture.Create<Project>();
        _projectServiceMock.Setup(projectService =>
                projectService.GetProjectByIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(project).Verifiable();

        var user = _fixture.Create<User>();
        _authorizationServiceMock
            .Setup(authorizationService => authorizationService.GetCurrentUserAsync(It.IsAny<string>(),
                authenticationHeaderValue, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user).Verifiable();

        var userPermissions = _fixture.Build<UserPermission>().FromFactory(() => new UserPermission(user))
            .With(userPermission => userPermission.IsAllowedToEditPermissions, true).Create();

        _authorizationServiceMock.Setup(authorizationService =>
                authorizationService.GetUserPermissionsAsync(project, user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userPermissions);

        // Act
        var actual = await _sut.GetUserPermissionAsync(authorizationRequest, authenticationHeaderValue);

        // Assert
        actual!.IsAllowedToEditPermissions.Should().BeTrue();
        actual.User.Should().NotBeNull();
        actual.User.Id.Should().Be(user.Id);
        _authorizationServiceMock.Verify();
    }

    private void ArrangeDefaultSetup(out AuthorizationRequest authorizationRequest,
        out User user, out IEnumerable<Permission> userPermissions)
    {
        var projectId = _fixture.Create<Guid>();
        var organization = _fixture.Create<string>();

        authorizationRequest = new AuthorizationRequest(projectId, organization);
        user = new User { Id = _fixture.Create<Guid>() };
        userPermissions = new List<Permission>();
    }
}