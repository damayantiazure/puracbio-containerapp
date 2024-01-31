using AutoMapper;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Tests.FixtureCustomizations;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.Mapping;
using Rabobank.Compliancy.Infrastructure.Permissions;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;
using Rabobank.Compliancy.Tests;
using Permission = Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models.Permission;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class ProtectedResourcePermissionsServiceTests : UnitTestBase
{
    private readonly IFixture _fixture;
    private readonly Mock<ILogQueryService> _logQueryServiceMock;
    private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
    private readonly Mock<IPermissionGroupService> _permissionGroupServiceMock;
    private readonly Mock<IPermissionContextFactory> _contextFactoryMock;
    private readonly Mock<IPermissionContextForResource<IProtectedResource>> _contextMock;
    private readonly Mock<IProtectedResource> _protectedResourceMock;
    private readonly IMapper _mapper;

    private ProtectedResourcePermissionsService _service;

    public ProtectedResourcePermissionsServiceTests()
    {
        _fixture = new Fixture();
        _fixture.Customize(new IdentityIsAlwaysUser());

        _logQueryServiceMock = new Mock<ILogQueryService>();
        _permissionRepositoryMock = new Mock<IPermissionRepository>();
        _permissionGroupServiceMock = new Mock<IPermissionGroupService>();
        _contextFactoryMock = new Mock<IPermissionContextFactory>();
        _contextMock = new Mock<IPermissionContextForResource<IProtectedResource>>();
        _protectedResourceMock = new Mock<IProtectedResource>();

        _mapper = CreateMapper();

        _contextMock
            .Setup(x => x.Resource)
            .Returns(_protectedResourceMock.Object);

        _contextFactoryMock
            .Setup(x => x.CreateContext<IProtectedResource>(_protectedResourceMock.Object))
            .Returns(_contextMock.Object);

        _service = new ProtectedResourcePermissionsService(_logQueryServiceMock.Object, _permissionRepositoryMock.Object, _mapper, _permissionGroupServiceMock.Object, _contextFactoryMock.Object);
    }

    [Fact]
    public async Task GetProductionDeploymentsAsync_ShouldReturnExpectedResult()
    {
        // Arrange
        var deploymentInformation = _fixture.Create<DeploymentInformation>();
        var queries = new List<string> { "query1", "query2" };

        _contextFactoryMock.Setup(x => x.CreateContext<IProtectedResource>(_protectedResourceMock.Object)).Returns(_contextMock.Object);
        _contextMock.Setup(x => x.GetRetentionQuery(It.IsAny<TimeSpan>())).Returns(queries);
        _logQueryServiceMock.Setup(x => x.GetQueryEntryAsync<DeploymentInformation>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(deploymentInformation);

        // Act
        var actual = await _service.GetProductionDeploymentAsync<IProtectedResource>(_protectedResourceMock.Object, TimeSpan.FromDays(10));

        // Assert
        actual.Should().BeEquivalentTo(deploymentInformation);
    }

    [Fact]
    public async Task GetProductionDeploymentsAsync_ShouldReturnDefault_WhenNoDeploymentsFound()
    {
        // Arrange
        var queries = new List<string> { "query1", "query2" };

        _contextFactoryMock.Setup(x => x.CreateContext<IProtectedResource>(_protectedResourceMock.Object)).Returns(_contextMock.Object);
        _contextMock.Setup(x => x.GetRetentionQuery(It.IsAny<TimeSpan>())).Returns(queries);
        _logQueryServiceMock.Setup(x => x.GetQueryEntriesAsync<DeploymentInformation>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<DeploymentInformation>());

        // Act
        var actual = await _service.GetProductionDeploymentAsync<IProtectedResource>(_protectedResourceMock.Object, TimeSpan.FromDays(10));

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public async Task GetProductionDeploymentsAsync_ShouldReturnFirstNonEmptyResult()
    {
        // Arrange
        var queries = new List<string> { "query1", "query2" };
        var deploymentInformationQuery2 = _fixture.Create<DeploymentInformation>();

        _contextFactoryMock.Setup(x => x.CreateContext<IProtectedResource>(_protectedResourceMock.Object)).Returns(_contextMock.Object);
        _contextMock.Setup(x => x.GetRetentionQuery(It.IsAny<TimeSpan>())).Returns(queries);
        _logQueryServiceMock.SetupSequence(x => x.GetQueryEntryAsync<DeploymentInformation>(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeploymentInformation?)null)
            .ReturnsAsync(deploymentInformationQuery2);

        // Act
        var actual = await _service.GetProductionDeploymentAsync<IProtectedResource>(_protectedResourceMock.Object, TimeSpan.FromDays(10));

        // Assert
        actual.Should().BeEquivalentTo(deploymentInformationQuery2);
    }

    [Fact]
    public async Task OpenPermissionedResourceAsync_ShouldUpdatePermissionsSuccessfully()
    {
        // Arrange
        var groupId = _fixture.Create<Guid>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var permissionBitsInScope = _fixture.Create<List<int>>();
        var permission = new Permission { PermissionBit = permissionBitsInScope.FirstOrDefault(), PermissionToken = _fixture.Create<string>() };
        var allPermissionsForIdentity = new PermissionsSet { Permissions = new List<Permission> { permission } };

        SetupCommonContext(new List<Guid> { groupId }, organization, projectId, permissionBitsInScope, allPermissionsForIdentity);

        var capturedContent = _fixture.Create<UpdatePermissionBodyContent>();
        _permissionRepositoryMock
            .Setup(x => x.UpdatePermissionGroupAsync(organization, projectId, It.IsAny<UpdatePermissionBodyContent>(), It.IsAny<CancellationToken>()))
            .Callback<string, Guid, UpdatePermissionBodyContent, CancellationToken>((org, proj, content, token) => capturedContent = content)
            .ReturnsAsync(allPermissionsForIdentity);

        _service = new ProtectedResourcePermissionsService(_logQueryServiceMock.Object, _permissionRepositoryMock.Object, _mapper, _permissionGroupServiceMock.Object, _contextFactoryMock.Object);

        // Act
        await _service.OpenPermissionedResourceAsync<IProtectedResource>(_protectedResourceMock.Object);

        // Assert
        _permissionRepositoryMock.Verify(repo =>
            repo.UpdatePermissionGroupAsync(organization, projectId, It.IsAny<UpdatePermissionBodyContent>(), It.IsAny<CancellationToken>()), Times.Once);
        capturedContent.UpdatePackage.Should().NotBeNull();
        capturedContent.UpdatePackage?.Contains(permission.PermissionToken).Should().BeTrue();
        capturedContent.UpdatePackage?.Contains(groupId.ToString()).Should().BeTrue();
        capturedContent.UpdatePackage?.Contains(groupId.ToString()).Should().BeTrue();
    }

    [Fact]
    public async Task OpenPermissionedResourceAsync_ShouldNotUpdatePermissions_WhenNoPermissionsInScope()
    {
        // Arrange
        var groupId = _fixture.Create<Guid>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();

        SetupCommonContext(new List<Guid> { groupId }, organization, projectId);

        // Act
        await _service.OpenPermissionedResourceAsync<IProtectedResource>(_protectedResourceMock.Object);

        // Assert
        _permissionRepositoryMock
            .Verify(x =>
                x.UpdatePermissionGroupAsync(organization, projectId, It.IsAny<UpdatePermissionBodyContent>(), It.IsAny<CancellationToken>()),
                Times.Never);
    }

    [Fact]
    public async Task OpenPermissionedResourceAsync_ShouldUpdatePermissions_ForMultipleGroupIds()
    {
        // Arrange
        var groupIds = _fixture.CreateMany<Guid>(2).ToList();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var permissionBitsInScope = _fixture.Create<List<int>>();
        var permission = new Permission { PermissionBit = permissionBitsInScope.FirstOrDefault(), PermissionToken = _fixture.Create<string>() };
        var allPermissionsForIdentity = new PermissionsSet { Permissions = new List<Permission> { permission } };

        SetupCommonContext(groupIds, organization, projectId, permissionBitsInScope, allPermissionsForIdentity); // Note: You would need to modify SetupCommonContext to handle multiple group IDs.

        // Act
        await _service.OpenPermissionedResourceAsync<IProtectedResource>(_protectedResourceMock.Object);

        // Assert
        _permissionRepositoryMock.Verify(
            x => x.UpdatePermissionGroupAsync(organization, projectId, It.IsAny<UpdatePermissionBodyContent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(groupIds.Count));
    }

    [Fact]
    public async Task OpenPermissionedResourceAsync_ShouldUpdateMultiplePermissions_WhenMultiplePermissionsInScope()
    {
        // Arrange
        var groupId = _fixture.Create<Guid>();
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>();
        var permissionBitsInScope = _fixture.Create<List<int>>();
        var allPermissionsForIdentity = new PermissionsSet
        {
            Permissions = permissionBitsInScope.Select(bit => new Permission
            {
                PermissionBit = bit,
                PermissionToken = _fixture.Create<string>()
            }).ToList()
        };

        SetupCommonContext(new List<Guid> { groupId }, organization, projectId, permissionBitsInScope, allPermissionsForIdentity);

        // Act
        await _service.OpenPermissionedResourceAsync<IProtectedResource>(_protectedResourceMock.Object);

        // Assert
        _permissionRepositoryMock.Verify(x =>
                x.UpdatePermissionGroupAsync(organization, projectId, It.IsAny<UpdatePermissionBodyContent>(), It.IsAny<CancellationToken>()),
            Times.Exactly(permissionBitsInScope.Count));
    }

    private void SetupCommonContext(List<Guid> groupIds, string organization, Guid projectId, List<int>? permissionBitsInScope = null, PermissionsSet? permissionsForIdentity = null)
    {
        var project = _fixture.Build<Project>()
            .With(p => p.Organization, organization)
            .With(p => p.Id, projectId)
            .Create();

        _protectedResourceMock
            .Setup(r => r.Project)
            .Returns(project);

        _contextMock.Setup(x => x.GetPermissionBitsInScope())
            .Returns(permissionBitsInScope ?? new List<int>());

        _permissionGroupServiceMock
            .Setup(x => x.GetUniqueIdentifiersForNativeAzdoGroups(It.IsAny<IPermissionContextForResource<IProtectedResource>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(groupIds);

        foreach (var groupId in groupIds)
        {
            _contextMock
                .Setup(x =>
                    x.GetPermissionsForIdentityAsync(groupId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(permissionsForIdentity);
        }
    }

    private static IMapper CreateMapper() =>
        new Mapper(new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<UpdatePermissionEntityMappingProfile>();
            cfg.AddProfile<UpdatePermissionBodyMappingProfile>();
        }));
}