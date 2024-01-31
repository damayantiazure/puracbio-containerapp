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

public class OpenBuildPipelinePermissionsProcessTests
{
    private readonly OpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline> _sut;

    private static readonly Mock<IProtectedResourcePermissionsService> _protectedResourcePermissionsServiceMock = new();
    private static readonly Mock<IProjectService> _projectServiceMock = new();
    private static readonly Mock<IPipelineService> _pipelineServiceMock = new();

    private readonly IFixture _fixture = new Fixture();

    public OpenBuildPipelinePermissionsProcessTests()
    {
        _sut = new OpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline>(_protectedResourcePermissionsServiceMock.Object,
                   _projectServiceMock.Object, _pipelineServiceMock.Object);

        _fixture.Customize(new OpenPermissionsCustomization());
    }

    [Fact]
    public async Task OpenPermissionAsync_WithExistingProjectAndPipeline_ShouldGetProtectedResourcesForBuildDefinitions()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var buildDefinitionPipeline = _fixture.Create<AzdoBuildDefinitionPipeline>();
        var buildDefinitionRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(buildDefinitionRequest.Organization, buildDefinitionRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project).Verifiable();

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoBuildDefinitionPipeline>(project, buildDefinitionRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinitionPipeline).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(buildDefinitionRequest);

        // Assert
        _projectServiceMock.Verify();
        _pipelineServiceMock.Verify();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenBuildPipelineHasRetentionPeriod_ShouldThrowIsProductionItemException()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var buildDefinitionPipeline = _fixture.Create<AzdoBuildDefinitionPipeline>();
        var buildDefinitionRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>>();

        var deploymentInformation = _fixture.Create<DeploymentInformation>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(buildDefinitionRequest.Organization, buildDefinitionRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoBuildDefinitionPipeline>(project, buildDefinitionRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinitionPipeline);

        _protectedResourcePermissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<AzdoBuildDefinitionPipeline>(buildDefinitionPipeline,
            PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>())).ReturnsAsync(deploymentInformation).Verifiable();

        // Act
        var actual = () => _sut.OpenPermissionAsync(buildDefinitionRequest);

        // Assert
        await actual.Should().ThrowAsync<IsProductionItemException>();
        _protectedResourcePermissionsServiceMock.Verify();
    }

    [Fact]
    public async Task OpenPermissionAsync_WhenBuildPipelineDoesNotHaveRetentionPeriod_ShouldOpenThePipelinePermissions()
    {
        // Arrange
        var project = _fixture.Create<Project>();
        var buildDefinitionPipeline = _fixture.Create<AzdoBuildDefinitionPipeline>();
        var buildDefinitionRequest = _fixture.Create<OpenPipelinePermissionsRequest<AzdoBuildDefinitionPipeline>>();

        _projectServiceMock.Setup(x => x.GetProjectByIdAsync(buildDefinitionRequest.Organization, buildDefinitionRequest.ProjectId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(project);

        _pipelineServiceMock.Setup(x => x.GetPipelineAsync<AzdoBuildDefinitionPipeline>(project, buildDefinitionRequest.PipelineId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(buildDefinitionPipeline);

        _protectedResourcePermissionsServiceMock.Setup(x => x.GetProductionDeploymentAsync<AzdoBuildDefinitionPipeline>(buildDefinitionPipeline,
            PipelineConstants.ReleasePipelineRetentionPeriodInDays, It.IsAny<CancellationToken>())).ReturnsAsync((DeploymentInformation)null);

        _protectedResourcePermissionsServiceMock.Setup(x => x.OpenPermissionedResourceAsync<AzdoBuildDefinitionPipeline>(buildDefinitionPipeline,
            It.IsAny<CancellationToken>())).Verifiable();

        // Act
        await _sut.OpenPermissionAsync(buildDefinitionRequest);

        // Assert
        _protectedResourcePermissionsServiceMock.Verify();
    }
}