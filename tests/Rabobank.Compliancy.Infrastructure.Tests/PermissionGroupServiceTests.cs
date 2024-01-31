using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infrastructure.InternalServices;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;
using Rabobank.Compliancy.Tests;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class PermissionGroupServiceTests : UnitTestBase
{
    private readonly IFixture _fixture;
    private readonly Mock<IApplicationGroupRepository> _applicationGroupRepositoryMock = new();
    private readonly Mock<IPermissionContextForResource<IProtectedResource>> _contextMock = new();
    private readonly Mock<IProtectedResource> _protectedResourceMock = new();
    private PermissionGroupService _sut;

    public PermissionGroupServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new IdentityIsAlwaysUser());

        _contextMock
            .Setup(x => x.Resource)
            .Returns(_protectedResourceMock.Object);

        _sut = new PermissionGroupService(_applicationGroupRepositoryMock.Object);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsScopedApplicationGroups()
    {
        // Arrange
        var permissionRepositoryMock = SetupApplicationGroupRepositoryMock();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        _sut = new PermissionGroupService(permissionRepositoryMock.Object);

        // Act
        await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, CancellationToken.None);

        // Assert
        permissionRepositoryMock.Verify(x =>
                x.GetScopedApplicationGroupForProjectAsync(organization, projectId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsExpectedIds()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        var cancellationToken = new CancellationToken();
        var permissionGroups = CreatePermissionGroupsWithDisplayNames().ToList();
        var displayNames = permissionGroups.Select(x => x.FriendlyDisplayName).ToList();
        _contextMock
            .Setup(x => x.GetNativeAzureDevOpsSecurityDisplayNames())
            .Returns(displayNames);
        var expectedIds = permissionGroups.Take(3).Select(i => i.TeamFoundationId).ToList();
        var applicationGroup = new ApplicationGroup { Identities = permissionGroups };

        _applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken))
            .ReturnsAsync(applicationGroup);

        // Act
        var result = await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, cancellationToken);

        // Assert
        Assert.Equal(expectedIds, result);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsEmptyList_WhenNoMatchesFound()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        var cancellationToken = new CancellationToken();
        var permissionGroups = CreatePermissionGroups(5).ToList();
        var applicationGroup = new ApplicationGroup { Identities = permissionGroups };

        _applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken))
            .ReturnsAsync(applicationGroup);

        // Act
        var result = await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, cancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsEmptyList_WhenApplicationGroupIsNull()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        var cancellationToken = new CancellationToken();

        _ = _applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken))
            .ReturnsAsync((ApplicationGroup?)null);

        // Act
        var result = await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, cancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsEmptyList_WhenIdentitiesIsNull()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        var cancellationToken = new CancellationToken();
        var applicationGroup = new ApplicationGroup { Identities = null };

        _applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken))
            .ReturnsAsync(applicationGroup);

        // Act
        var result = await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, cancellationToken);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task When_GetUniqueIdentifiersForNativeAzdoGroups_ReturnsEmptyList_WhenIdentitiesIsEmpty()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        SetupCommonContext(organization, projectId);

        var cancellationToken = new CancellationToken();
        var applicationGroup = new ApplicationGroup { Identities = new List<PermissionGroup>() };
        _applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken))
            .ReturnsAsync(applicationGroup);

        // Act
        var result = await _sut.GetUniqueIdentifiersForNativeAzdoGroups(_contextMock.Object, cancellationToken);

        // Assert
        Assert.Empty(result);
    }

    private IEnumerable<PermissionGroup> CreatePermissionGroupsWithDisplayNames()
    {
        return new List<PermissionGroup>
        {
            new()
            {
                TeamFoundationId = _fixture.Create<Guid>(),
                FriendlyDisplayName = NativeAzureDevOpsSecurityDisplayNames.ProjectAdministrators,
                DisplayName = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                IdentityType = _fixture.Create<string>(),
                IsProjectLevel = _fixture.Create<bool>()
            },
            new()
            {
                TeamFoundationId = _fixture.Create<Guid>(),
                FriendlyDisplayName = NativeAzureDevOpsSecurityDisplayNames.BuildAdministrators,
                DisplayName = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                IdentityType = _fixture.Create<string>(),
                IsProjectLevel = _fixture.Create<bool>()
            },
            new()
            {
                TeamFoundationId = _fixture.Create<Guid>(),
                FriendlyDisplayName = NativeAzureDevOpsSecurityDisplayNames.Contributors,
                DisplayName = _fixture.Create<string>(),
                Description = _fixture.Create<string>(),
                IdentityType = _fixture.Create<string>(),
                IsProjectLevel = _fixture.Create<bool>()
            }
        };
    }

    private IEnumerable<PermissionGroup> CreatePermissionGroups(int count)
    {
        return _fixture.Build<PermissionGroup>().CreateMany(count);
    }

    private void SetupCommonContext(string organization, Guid projectId)
    {
        var project = _fixture.Build<Project>()
            .With(p => p.Organization, organization)
            .With(p => p.Id, projectId)
            .Create();

        _protectedResourceMock
            .Setup(r => r.Project)
            .Returns(project);
    }

    private Mock<IApplicationGroupRepository> SetupApplicationGroupRepositoryMock(List<PermissionGroup>? scopedIdentities = null)
    {
        scopedIdentities ??= new List<PermissionGroup>();

        var applicationGroupRepositoryMock = new Mock<IApplicationGroupRepository>();

        var applicationGroupScoped = _fixture.Build<ApplicationGroup>()
            .With(ag => ag.Identities, scopedIdentities)
            .Create();

        applicationGroupRepositoryMock.Setup(repo => repo.GetScopedApplicationGroupForProjectAsync(
                It.IsAny<string>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(applicationGroupScoped);

        return applicationGroupRepositoryMock;
    }
}