using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.Repositories;

public class PermissionRepositoryTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IDevHttpClientCallHandler> _httpClientCallHandlerMock = new();

    private readonly PermissionRepository _sut;

    public PermissionRepositoryTests()
    {
        _sut = new PermissionRepository(_httpClientCallHandlerMock.Object);
    }

    [Fact]
    public async Task AddMembersToGroupsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionsSetId = _fixture.Create<MembersGroupResponse?>();

        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<MembersGroupResponse?, AddMemberData>(It.IsAny<Uri>(), It.IsAny<AddMemberData>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionsSetId).Verifiable();

        // Act
        var actual = await _sut.AddMembersToGroupsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.CreateMany<Guid>(), _fixture.CreateMany<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionsSetId);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task CreateManageGroupAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var group = _fixture.Create<Group?>();

        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<Group?, ManageGroup>(It.IsAny<Uri>(), It.IsAny<ManageGroup>(),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(group).Verifiable();

        // Act
        var actual = await _sut.CreateManageGroupAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(group);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetProjectGroupAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var projectGroup = _fixture.Create<ProjectGroup?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<ProjectGroup?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(projectGroup).Verifiable();

        // Act
        var actual = await _sut.GetProjectGroupAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(projectGroup);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetPermissionsUserOrGroup_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionProjectId = _fixture.Create<PermissionsProjectId?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PermissionsProjectId?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionProjectId).Verifiable();

        // Act
        var actual = await _sut.GetPermissionsUserOrGroupAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionProjectId);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task UpdatePermissionGroupAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var group = _fixture.Create<PermissionsSet?>();
        var expectedBody = _fixture.Create<UpdatePermissionBodyContent>();

        _httpClientCallHandlerMock.Setup(x => x.HandlePostCallAsync<PermissionsSet?, UpdatePermissionBodyContent>(It.IsAny<Uri>(), It.Is<UpdatePermissionBodyContent>(body => body.UpdatePackage == expectedBody.UpdatePackage),
                It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(group).Verifiable();

        // Act
        var actual = await _sut.UpdatePermissionGroupAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), expectedBody, It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(group);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetRepositoryDisplayPermissionsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionProjectId = _fixture.Create<PermissionsSet?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PermissionsSet?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionProjectId).Verifiable();

        // Act
        var actual = await _sut.GetRepositoryDisplayPermissionsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionProjectId);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetReleaseDefinitionDisplayPermissionsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionProjectId = _fixture.Create<PermissionsSet?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PermissionsSet?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionProjectId).Verifiable();

        // Act
        var actual = await _sut.GetReleaseDefinitionDisplayPermissionsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionProjectId);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetBuildDefinitionDisplayPermissionsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionProjectId = _fixture.Create<PermissionsSet?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PermissionsSet?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionProjectId).Verifiable();

        // Act
        var actual = await _sut.GetBuildDefinitionDisplayPermissionsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionProjectId);
        _httpClientCallHandlerMock.Verify();
    }

    [Fact]
    public async Task GetApplicationGroupDisplayPermissionsAsync_WithCorrectlyFilledParameters_ShouldBeEquivalentToExpectedResponse()
    {
        // Arrange
        var permissionProjectId = _fixture.Create<PermissionsSet?>();

        _httpClientCallHandlerMock.Setup(x => x.HandleGetCallAsync<PermissionsSet?>(It.IsAny<Uri>(), It.IsAny<AuthenticationHeaderValue>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(permissionProjectId).Verifiable();

        // Act
        var actual = await _sut.GetApplicationGroupDisplayPermissionsAsync(_fixture.Create<string>(), _fixture.Create<Guid>(), _fixture.Create<Guid>(), It.IsAny<CancellationToken>());

        // Assert
        actual.Should().BeEquivalentTo(permissionProjectId);
        _httpClientCallHandlerMock.Verify();
    }
}