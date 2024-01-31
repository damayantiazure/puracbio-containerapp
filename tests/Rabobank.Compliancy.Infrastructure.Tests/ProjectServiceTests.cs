using Microsoft.TeamFoundation.Core.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class ProjectServiceTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IProjectRepository> _projectRepositoryMock = new();
    private readonly Mock<IAccessControlListsRepository> _aclListsRepositoryMock = new();
    private readonly Mock<IRecursiveIdentityCacheBuilder> _recursiveIdentityCacheBuilderMock = new();
    private readonly ProjectService _sut;

    public ProjectServiceTests()
    {
        _sut = new(_projectRepositoryMock.Object, _aclListsRepositoryMock.Object, _recursiveIdentityCacheBuilderMock.Object);
    }

    [Fact]
    public async Task GetProjectByIdAsync_WithCorrectInput_ReturnsExpectedResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var expectedTeamProject = _fixture.Create<TeamProject>();
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(organization, expectedTeamProject.Id, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTeamProject);

        // Act
        var actual = await _sut.GetProjectByIdAsync(organization, expectedTeamProject.Id);

        // Assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expectedTeamProject.Id);
        actual.Name.Should().Be(expectedTeamProject.Name);
        actual.Organization.Should().Be(organization);
    }

    [Fact]
    public async Task GetProjectByIdAsync_HttpRequestExceptionWithNotFoundStatusCode_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound));

        // Act
        var act = async () => { await _sut.GetProjectByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetProjectByIdAsync_RandomException_ThrowsSameException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = async () => { await _sut.GetProjectByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetProjectByIdAsync_HttpRequestExceptionWithBadGatewayStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByIdAsync(It.IsAny<string>(), It.IsAny<Guid>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.BadGateway));

        // Act
        var act = async () => { await _sut.GetProjectByIdAsync(_fixture.Create<string>(), _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetProjectByNameAsync_WithCorrectInput_ReturnsExpectedResult()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var expectedTeamProject = _fixture.Create<TeamProject>();
        _projectRepositoryMock.Setup(r => r.GetProjectByNameAsync(organization, expectedTeamProject.Name, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTeamProject);

        // Act
        var actual = await _sut.GetProjectByNameAsync(organization, expectedTeamProject.Name);

        // Assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expectedTeamProject.Id);
        actual.Name.Should().Be(expectedTeamProject.Name);
    }

    [Fact]
    public async Task GetProjectByNameAsync_HttpRequestExceptionWithNotFoundStatusCode_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByNameAsync(It.IsAny<string>(), It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound));

        // Act
        var act = async () => { await _sut.GetProjectByNameAsync(_fixture.Create<string>(), _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetProjectByNameAsync_HttpRequestExceptionWithBadGatewayStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByNameAsync(It.IsAny<string>(), It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.BadGateway));

        // Act
        var act = async () => { await _sut.GetProjectByNameAsync(_fixture.Create<string>(), _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetProjectByNameAsync_RandomException_ThrowsSameException()
    {
        // Arrange
        _projectRepositoryMock.Setup(r => r.GetProjectByNameAsync(It.IsAny<string>(), It.IsAny<string>(), false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = async () => { await _sut.GetProjectByNameAsync(_fixture.Create<string>(), _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}