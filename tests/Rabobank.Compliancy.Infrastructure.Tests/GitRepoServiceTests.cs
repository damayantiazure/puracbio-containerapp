using Microsoft.TeamFoundation.SourceControl.WebApi;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Tests;

namespace Rabobank.Compliancy.Infrastructure.Tests;

public class GitRepoServiceTests : UnitTestBase
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IGitRepositoryRepository> _gitRepoRepository = new();
    private readonly Mock<IAccessControlListsRepository> _accessControlListsRepository = new();
    private readonly Mock<IRecursiveIdentityCacheBuilder> _recursiveIdentityCacheBuilderMock = new();
    private readonly GitRepoService _sut;

    public GitRepoServiceTests()
    {
        _sut = new(_gitRepoRepository.Object, _accessControlListsRepository.Object, _recursiveIdentityCacheBuilderMock.Object);
    }

    [Fact]
    public async Task GetGitRepoByIdAsync_WithCorrectIdInput_ReturnsExpectedResult()
    {
        // Arrange
        var project = CreateDefaultProject();
        var expectedGitRepos = SetupDefaultMock(project);
        var expectedGitRepoId = expectedGitRepos[1].Id;

        // Act
        var actual = await _sut.GetGitRepoByIdAsync(project, expectedGitRepoId);

        // Assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expectedGitRepos[1].Id);
        actual.Name.Should().Be(expectedGitRepos[1].Name);
        actual.Url.Should().Be(new Uri(expectedGitRepos[1].Url));
    }

    [Fact]
    public async Task GetGitRepoByIdAsync_WithCorrectIdInputButWithoutResult_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        var project = CreateDefaultProject();

        // Act
        var act = async () => { await _sut.GetGitRepoByIdAsync(project, _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetGitRepoByIdAsync_HttpRequestExceptionWithNotFoundStatusCode_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        var project = CreateDefaultProject();
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound));

        // Act
        var act = async () => { await _sut.GetGitRepoByIdAsync(project, _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetGitRepoByIdAsync_HttpRequestExceptionWithBadGatewayStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var project = CreateDefaultProject();
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.BadGateway));

        // Act
        var act = async () => { await _sut.GetGitRepoByIdAsync(project, _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetGitRepoByIdAsync_RandomException_ThrowsSameException()
    {
        // Arrange
        var project = CreateDefaultProject();
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException());

        // Act
        var act = async () => { await _sut.GetGitRepoByIdAsync(project, _fixture.Create<Guid>()); };

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_WithCorrectNameInput_ReturnsExpectedResult()
    {
        // Arrange
        var project = CreateDefaultProject();
        var expectedGitRepos = SetupDefaultMock(project);
        var expectedGitRepoName = expectedGitRepos[1].Name;

        // Act
        var actual = await _sut.GetGitRepoByNameAsync(project, expectedGitRepoName);

        // Assert
        actual.Should().NotBeNull();
        actual.Id.Should().Be(expectedGitRepos[1].Id);
        actual.Name.Should().Be(expectedGitRepos[1].Name);
        actual.Url.Should().Be(new Uri(expectedGitRepos[1].Url));
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_WithCorrectNameInputButWithoutResult_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        var project = CreateDefaultProject();

        // Act
        var act = async () => { await _sut.GetGitRepoByNameAsync(project, _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_HttpRequestExceptionWithNotFoundStatusCode_ThrowsSourceItemNotFoundException()
    {
        // Arrange
        var project = CreateDefaultProject();
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.NotFound));

        // Act
        var act = async () => { await _sut.GetGitRepoByNameAsync(project, _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<SourceItemNotFoundException>();
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_HttpRequestExceptionWithBadGatewayStatusCode_ThrowsHttpRequestException()
    {
        // Arrange
        var project = CreateDefaultProject();
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException(_fixture.Create<string>(), null, HttpStatusCode.BadGateway));

        // Act
        var act = async () => { await _sut.GetGitRepoByNameAsync(project, _fixture.Create<string>()); };

        // Assert
        await act.Should().ThrowAsync<HttpRequestException>();
    }

    [Fact]
    public async Task GetGitRepoByNameAsync_WhenCalledTwice_UsesCache()
    {
        // Arrange
        var project = CreateDefaultProject();
        var expectedGitRepos = SetupDefaultMock(project);
        var expectedGitRepoName = expectedGitRepos[1].Name;

        // Act
        var sut = _sut;
        var actual1 = await sut.GetGitRepoByNameAsync(project, expectedGitRepoName);
        var actual2 = await sut.GetGitRepoByNameAsync(project, expectedGitRepoName);

        // Assert
        actual1.Should().NotBeNull();
        actual1.Id.Should().Be(expectedGitRepos[1].Id);
        actual2.Id.Should().Be(expectedGitRepos[1].Id);
        _gitRepoRepository.Verify(r => r.GetGitReposByProjectAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPermissionsAsync_WithoutAnyPermissions_ReturnsEmptyEnumerables()
    {
        // Arrange
        var gitRepo = new GitRepo() { Project = CreateDefaultProject() };

        // Act
        var actual = await _sut.AddPermissionsAsync(gitRepo, default);

        // Assert
        actual.Should().NotBeNull();
        actual.Permissions.Should().BeEmpty();
    }

    [Fact]
    public async Task AddPermissionsAsync_WithoutNotApplicablePermissions_ReturnsEmptyEnumerables()
    {
        // Arrange
        var gitRepo = new GitRepo() { Project = CreateDefaultProject() };
        _accessControlListsRepository.Setup(x =>
                x.GetAccessControlListsForProjectAndSecurityNamespaceAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessControlList[] { new AccessControlList(
                InvariantUnitTestValue,
                false,
                new Dictionary<IdentityDescriptor, AccessControlEntry>
                {
                    {
                        new IdentityDescriptor(InvariantUnitTestValue, InvariantUnitTestValue),
                        new AccessControlEntry(new IdentityDescriptor(InvariantUnitTestValue, InvariantUnitTestValue), 0, 0, null)
                    }
                },
                false)
            });

        // Act
        var actual = await _sut.AddPermissionsAsync(gitRepo, default);

        // Assert
        actual.Should().NotBeNull();
        actual.Permissions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(8192)] // Manage
    [InlineData(512)] // Delete
    [InlineData(8704)] // Both
    public async Task AddPermissionsAsync_WithApplicablePermissions_ReturnsIdentities(int bit)
    {
        // Arrange
        var gitRepo = new GitRepo() { Project = CreateDefaultProject() };
        _accessControlListsRepository.Setup(x =>
                x.GetAccessControlListsForProjectAndSecurityNamespaceAsync(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AccessControlList[] { new AccessControlList(
                InvariantUnitTestValue,
                false,
                new Dictionary<IdentityDescriptor, AccessControlEntry>
                {
                    {
                        new IdentityDescriptor(InvariantUnitTestValue, InvariantUnitTestValue),
                        new AccessControlEntry(new IdentityDescriptor(InvariantUnitTestValue, InvariantUnitTestValue), bit, 0, new AceExtendedInformation(bit, 0, bit, 0))
                    }
                },
                false)
            });

        // Act
        var actual = await _sut.AddPermissionsAsync(gitRepo, default);

        // Assert
        actual.Should().NotBeNull();
        actual.Permissions.Should().NotBeEmpty();
    }

    private List<GitRepository> SetupDefaultMock(Project project)
    {
        var expectedGitRepos = _fixture.CreateMany<GitRepository>().ToList();
        expectedGitRepos.ForEach(g => g.Url = "http://www.unittest.nl/");
        _gitRepoRepository.Setup(r => r.GetGitReposByProjectAsync(project.Organization, project.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedGitRepos);

        return expectedGitRepos;
    }

    private Project CreateDefaultProject()
    {
        return _fixture.Build<Project>().Without(p => p.Permissions).Create();
    }
}