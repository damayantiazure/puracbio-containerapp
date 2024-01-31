using AutoFixture.Kernel;
using Microsoft.VisualStudio.Services.Location;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using System.Net.Http.Headers;
using AzdoModels = Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Identity = Microsoft.VisualStudio.Services.Identity.Identity;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class AuthorizationServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IAuthorizationRepository> _authorizationRepositoryMock = new();
    private readonly Mock<IPermissionRepository> _permissionsRepositoryMock = new();
    private readonly AuthorizationService _sut;

    public AuthorizationServiceTests()
    {
        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        _fixture.Customizations.Add(
            new TypeRelay(
                typeof(PipelineResource),
                typeof(Pipeline))
        );
        _fixture.Customizations.Add(
        new TypeRelay(
            typeof(ISettings),
            typeof(Pipeline)));

        _fixture.Customizations.Add(
          new TypeRelay(
              typeof(Domain.Compliancy.Authorizations.IIdentity),
              typeof(User)));

        _fixture.Customizations.Add(
           new TypeRelay(
               typeof(ITrigger),
               typeof(PipelineTrigger)));

        _fixture.Customize<Pipeline>(c => c
            .With(p => p.DefinitionType, PipelineProcessType.Yaml)
        );

        _sut = new AuthorizationService(_authorizationRepositoryMock.Object, _permissionsRepositoryMock.Object);
    }

    [Fact]
    public async Task GetCurrentUser_With_AccessToken_ReturnsUserId()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out var connectionData, out var userId);

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))
            .ReturnsAsync(connectionData);

        // Act
        var result = await _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        result.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetCurrentUser_With_UnauthorizedUser_Throws_TokenInvalidException()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out _, out _);

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))
            .ThrowsAsync(new HttpRequestException(string.Empty, null, HttpStatusCode.Unauthorized));

        // Act
        var actual = () => _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        await actual.Should().ThrowAsync<TokenInvalidException>();
    }

    [Fact]
    public async Task GetCurrentUser_With_UnsupportedMediaTypeException_Throws_TokenInvalidException()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out _, out _);

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))
            .ThrowsAsync(new UnsupportedMediaTypeException(string.Empty, new MediaTypeHeaderValue("text/html")));

        // Act
        var actual = () => _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        await actual.Should().ThrowAsync<TokenInvalidException>();
    }

    [Fact]
    public async Task GetCurrentUser_With_HttpRequestException_Throws_ArgumentException()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out _, out _);

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))
            .ThrowsAsync(new HttpRequestException(string.Empty, null, HttpStatusCode.BadRequest));

        // Act
        var actual = () => _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        await actual.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCurrentUser_With_AuthorizationData_Null_Throws_ArgumentExceptions()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out var connectionData, out _);
        connectionData = null;

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))!
            .ReturnsAsync(connectionData);

        // Act
        var actual = () => _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        await actual.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetCurrentUser_With_AuthorizedUser_Null_Throws_ArgumentExceptions()
    {
        // Arrange
        ArrangeDefaultSetup_GetCurrentUser(out var accessToken, out var organization, out var connectionData, out _);
        connectionData.AuthorizedUser = null;

        _authorizationRepositoryMock
            .Setup(_ => _.GetUserForAccessToken(accessToken, organization, default))
            .ReturnsAsync(connectionData);

        // Act
        var actual = () => _sut.GetCurrentUserAsync(organization, accessToken);

        // Assert
        await actual.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_CorrectPermissions_Returns_Mapped_DomainPermissions()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);
        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var result = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        var permissions = result.ToList();
        if (permissionsProjectId.Security?.Permissions is not null)
        {
            foreach (var permission in permissionsProjectId.Security.Permissions)
            {
                permissions.Exists(p => p.Name == permission.DisplayName
                                     && (int)p.Type == permission.PermissionId)
                    .Should().BeTrue();
            }
        }
        else
        {
            Assert.Fail("Permissions was null");
        }
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_Security_Null_Returns_EmptyEnumerable()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);
        permissionsProjectId.Security = null;
        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var result = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        result.Should().BeEquivalentTo(Enumerable.Empty<Domain.Compliancy.Permission>());
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_Permissions_Null_Returns_EmptyEnumerable()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);

        if (permissionsProjectId.Security is not null)
        {
            permissionsProjectId.Security.Permissions = null;
        }
        else
        {
            Assert.Fail("Security was null");
        }

        _permissionsRepositoryMock
        .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
        .ReturnsAsync(permissionsProjectId);

        // Act
        var result = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        result.Should().BeEquivalentTo(Enumerable.Empty<Domain.Compliancy.Permission>());
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_DescriptorIdentifier_Empty_Returns_EmptyEnumerable()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);
        if (permissionsProjectId.Security is not null)
        {
            permissionsProjectId.Security.DescriptorIdentifier = string.Empty;
        }
        else
        {
            Assert.Fail("Security was null");
        }
        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var result = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        result.Should().BeEquivalentTo(Enumerable.Empty<Domain.Compliancy.Permission>());
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_DescriptorIdentifier_Contains_ConflictSecurityDescriptor_Returns_EmptyEnumerable()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);

        if (permissionsProjectId.Security is not null)
        {
            permissionsProjectId.Security.DescriptorIdentifier = PermissionConstants.ConflictSecurityDescriptor;
        }
        else
        {
            Assert.Fail("Security was null");
        }

        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var result = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        result.Should().BeEquivalentTo(Enumerable.Empty<Domain.Compliancy.Permission>());
    }

    [Theory]
    [InlineData(0, PermissionType.NotSet)]
    [InlineData(1, PermissionType.Allow)]
    [InlineData(2, PermissionType.Deny)]
    [InlineData(3, PermissionType.AllowInherited)]
    [InlineData(4, PermissionType.DenyInherited)]
    [InlineData(5, PermissionType.AllowSystem)]
    public async Task GetPermissionsForUserOrGroup_With_PermissionId_Returns_PermissionTypeCorrectly(int permissionId, PermissionType expectedPermissionType)
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);
        foreach (var permission in permissionsProjectId.Security!.Permissions!)
        {
            permission.PermissionId = permissionId;
        }

        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var actual = await _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        actual.Should().Contain(permission => permission.Type == expectedPermissionType);
    }

    [Fact]
    public async Task GetPermissionsForUserOrGroup_With_Unsupported_PermissionId_Throws_InvalidOperationsExceptions()
    {
        // Arrange
        ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out var request, out var id, out var permissionsProjectId);

        foreach (var permission in permissionsProjectId.Security!.Permissions!)
        {
            // set unsupported permission id
            permission.PermissionId = -1;
        }

        _permissionsRepositoryMock
            .Setup(_ => _.GetPermissionsUserOrGroupAsync(request.Organization, request.ProjectId, id, default))
            .ReturnsAsync(permissionsProjectId);

        // Act
        var actual = () => _sut.GetPermissionsForUserOrGroupAsync(request.Organization, request.ProjectId, id);

        // Assert
        await actual.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithUserThatHasEditPermissions_ShouldReturnTrue()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var userId = _fixture.Create<Guid>();

        var permissionSetId = _fixture.Build<PermissionsSet>().With(permissionsSet => permissionsSet.CanEditPermissions, true).Create();
        var permissionProjectId = _fixture.Build<PermissionsProjectId>().With(permissionsProjectId => permissionsProjectId.Security, permissionSetId).Create();
        _permissionsRepositoryMock.Setup(x => x.GetPermissionsUserOrGroupAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(permissionProjectId);

        // Act
        var actual = await _sut.GetUserPermissionsAsync(project, userId);

        // Assert
        actual.Should().NotBeNull();
        actual!.IsAllowedToEditPermissions.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithUserThatDoesNotHaveEditPermissions_ShouldReturnFalse()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var userId = _fixture.Create<Guid>();

        var permissionSetId = _fixture.Build<PermissionsSet>().With(permissionsSet => permissionsSet.CanEditPermissions, false).Create();
        var permissionProjectId = _fixture.Build<PermissionsProjectId>().With(permissionsProjectId => permissionsProjectId.Security, permissionSetId).Create();
        _permissionsRepositoryMock.Setup(x => x.GetPermissionsUserOrGroupAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync(permissionProjectId);

        // Act
        var actual = await _sut.GetUserPermissionsAsync(project, userId);

        // Assert
        actual.Should().NotBeNull();
        actual!.IsAllowedToEditPermissions.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissionsAsync_WithNoDisplayPermissionsInstance_ShouldReturnNull()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var userId = _fixture.Create<Guid>();

        _permissionsRepositoryMock.Setup(repository => repository.GetPermissionsUserOrGroupAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), default))
            .ReturnsAsync((PermissionsProjectId?)null);

        // Act
        var actual = await _sut.GetUserPermissionsAsync(project, userId);

        // Assert
        actual.Should().BeNull();
    }

    private void ArrangeDefaultSetup_GetCurrentUser(out AuthenticationHeaderValue authenticationHeaderValue, out string organization, out ConnectionData connectionData, out Guid userId)
    {
        organization = _fixture.Create<string>();
        authenticationHeaderValue = new AuthenticationHeaderValue("scheme", "parameter");
        userId = _fixture.Create<Guid>();
        connectionData = new ConnectionData
        {
            AuthorizedUser = new Identity
            {
                Id = userId
            }
        };
    }

    private void ArrangeDefaultSetup_GetPermissionsForUserOrGroup(out AuthorizationRequest request, out Guid id, out PermissionsProjectId permissionsProjectId)
    {
        var projectId = _fixture.Create<Guid>();
        var organization = _fixture.Create<string>();

        request = new AuthorizationRequest(projectId, organization);

        id = _fixture.Create<Guid>();
        permissionsProjectId = new PermissionsProjectId
        {
            Security = new PermissionsSet
            {
                Permissions = new List<AzdoModels.Permission>
                {
                    new()
                    {
                        DisplayName = _fixture.Create<string>(),
                        PermissionId = (int)_fixture.Create<PermissionType>()
                    }
                },
                DescriptorIdentifier = _fixture.Create<string>()
            }
        };
    }
}