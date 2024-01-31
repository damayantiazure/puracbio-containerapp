using FluentAssertions;
using Rabobank.Compliancy.Application.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Application.Tests.Customizations;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Constants;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;

namespace Rabobank.Compliancy.Application.Tests.OpenPermissions;

public class OpenReleasePipelinePermissionsProcessTests
{
    private readonly OpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline> _sut;

    private readonly static Mock<IProtectedResourcePermissionsService> _protectedResourcePermissionsServiceMock = new();
    private readonly static Mock<IProjectService> _projectServiceMock = new();
    private readonly static Mock<IPipelineService> _pipelineServiceMock = new();

    private readonly IFixture _fixture = new Fixture();

    public OpenReleasePipelinePermissionsProcessTests()
    {
        _sut = new OpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline>(_protectedResourcePermissionsServiceMock.Object,
                   _projectServiceMock.Object, _pipelineServiceMock.Object);

        _fixture.Customize(new OpenPermissionsCustomization());
    }

    [Fact]
    public async Task OpenPermissionAsync_WithExistingProjectAndPipeline_ShouldGetProtectedResourcesForReleaseDefinitions()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var releaseDefinitionPipeline = _fixture.Create<AzdoReleaseDefinitionPipeline>();
        var releasePipelineRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(releasePipelineRequest.Organization, releasePipelineRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project).Verifiable();

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoReleaseDefinitionPipeline>(project, releasePipelineRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitionPipeline).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(releasePipelineRequest);

        // Assert
        _projectServiceMock.Verify();
        _pipelineServiceMock.Verify();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenReleasePipelineHasRetentionPeriod_ShouldThrowIsProductionItemException()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var releaseDefinitionPipeline = _fixture.Create<AzdoReleaseDefinitionPipeline>();
        var releaseDefinitionRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>>();

        var deploymentInformation = _fixture.Create<DeploymentInformation>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(releaseDefinitionRequest.Organization, releaseDefinitionRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoReleaseDefinitionPipeline>(project, releaseDefinitionRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitionPipeline);

        _protectedResourcePermissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<AzdoReleaseDefinitionPipeline>(releaseDefinitionPipeline,
            PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>())).ReturnsAsync(deploymentInformation).Verifiable();

        // Act
        var actual = () => _sut.OpenPermissionAsync(releaseDefinitionRequest);

        // Assert
        await actual.Should().ThrowAsync<IsProductionItemException>();
        _protectedResourcePermissionsServiceMock.Verify();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenReleasePipelineDoesNotHaveRetentionPeriod_ShouldOpenThePipelinePermissions()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var releaseDefinitionPipeline = _fixture.Create<AzdoReleaseDefinitionPipeline>();
        var releaseDefinitionRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoReleaseDefinitionPipeline>>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(releaseDefinitionRequest.Organization, releaseDefinitionRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoReleaseDefinitionPipeline>(project, releaseDefinitionRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(releaseDefinitionPipeline);

        _protectedResourcePermissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<AzdoReleaseDefinitionPipeline>(releaseDefinitionPipeline,
            PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>())).ReturnsAsync((DeploymentInformation)null);

        _protectedResourcePermissionsServiceMock.Setup(x => x.OpenPermissionedResourceAsync<AzdoReleaseDefinitionPipeline>(releaseDefinitionPipeline,
            It.IsAny<CancellationToken>())).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(releaseDefinitionRequest);

        // Assert
        _protectedResourcePermissionsServiceMock.Verify();
    }
}