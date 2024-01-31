using FluentAssertions;
using Rabobank.Compliancy.Application.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.Customizations;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Constants;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Tests.OpenPermissions;

public class OpenGitRepoPermissionsProcessTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IProtectedResourcePermissionsService> _permissionsServiceMock = new();
    private readonly Mock<IProjectService> _projectServiceMock = new();
    private readonly Mock<IGitRepoService> _gitRepoServiceMock = new();

    private readonly OpenGitRepoPermissionsProcess _sut;

    public OpenGitRepoPermissionsProcessTests()
    {
        _sut = new OpenGitRepoPermissionsProcess(_permissionsServiceMock.Object, _projectServiceMock.Object, _gitRepoServiceMock.Object);

        _fixture.Customize(new OpenPermissionsCustomization());
    }

    [Fact]
    public async Task OpenPermissionAsync_WithExistingProjectAndGitRepository_ShouldGetProtectedResourcesForGitRepository()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var gitRepo = _fixture.Create<GitRepo>();
        var openGitRepoPermissionsRequest = _fixture.Create<OpenGitRepoPermissionsRequest>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(openGitRepoPermissionsRequest.Organization, openGitRepoPermissionsRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project).Verifiable();

        _gitRepoServiceMock.Setup(x => x.GetGitRepoByIdAsync(project, openGitRepoPermissionsRequest.GitRepoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitRepo).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(openGitRepoPermissionsRequest);

        // Assert
        _projectServiceMock.Verify();
        _gitRepoServiceMock.Verify();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenGitRepositoryHasRetentionPeriod_ShouldThrowIsProductionItemException()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var gitRepo = _fixture.Create<GitRepo>();
        var openGitRepoPermissionsRequest = _fixture.Create<OpenGitRepoPermissionsRequest>();

        var deploymentInformation = _fixture.Create<DeploymentInformation>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(openGitRepoPermissionsRequest.Organization, openGitRepoPermissionsRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _gitRepoServiceMock.Setup(x => x.GetGitRepoByIdAsync(project, openGitRepoPermissionsRequest.GitRepoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitRepo);

        _permissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<GitRepo>(gitRepo, PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deploymentInformation);

        // Act
        var actual = () => _sut.OpenPermissionAsync(openGitRepoPermissionsRequest);

        // Assert
        await actual.Should().ThrowAsync<IsProductionItemException>();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenGitRepositoryDoesNotHaveRetentionPeriod_ShouldOpenTheGitRepositoryPermissions()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var gitRepo = _fixture.Create<GitRepo>();
        var openGitRepoPermissionsRequest = _fixture.Create<OpenGitRepoPermissionsRequest>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(openGitRepoPermissionsRequest.Organization, openGitRepoPermissionsRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _gitRepoServiceMock.Setup(x => x.GetGitRepoByIdAsync(project, openGitRepoPermissionsRequest.GitRepoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(gitRepo);

        _permissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<GitRepo>(gitRepo, PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DeploymentInformation)null);

        _permissionsServiceMock.Setup(x => x.OpenPermissionedResourceAsync<GitRepo>(gitRepo, It.IsAny<CancellationToken>())).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(openGitRepoPermissionsRequest);

        // Assert
        _permissionsServiceMock.Verify();
    }
}