using AutoFixture;
using Moq;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class YamlReleasePipelineFollowsMainframeCobolReleaseProcessTests
{
    private readonly IFixture _fixture = new Fixture();

    private readonly Mock<IPipelineEvaluatorFactory> _pipelineEvaluatorFactoryMock;
    private readonly Mock<IPipelineEvaluator> _pipelineEvaluatorMock;
    private readonly YamlReleasePipelineFollowsMainframeCobolReleaseProcess _sut;

    public YamlReleasePipelineFollowsMainframeCobolReleaseProcessTests()
    {
        _pipelineEvaluatorFactoryMock = new Mock<IPipelineEvaluatorFactory>();
        _pipelineEvaluatorMock = new Mock<IPipelineEvaluator>();

        _pipelineEvaluatorFactoryMock.Setup(x => x.Create(It.IsAny<BuildDefinition>()))
            .Returns(_pipelineEvaluatorMock.Object);

        _sut = new YamlReleasePipelineFollowsMainframeCobolReleaseProcess(null,
            _pipelineEvaluatorFactoryMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WithNoOrganization_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        // Act
        Func<Task<bool>> actual = () => _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoProjectId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = string.Empty;
        var buildDefinition = _fixture.Create<BuildDefinition>();

        // Act
        Func<Task<bool>> actual = () => _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoBuildDefinition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        BuildDefinition buildDefinition = null;

        // Act
        Func<Task<bool>> actual = () => _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithBuildDefinition_ShouldReturnTrue()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        _pipelineEvaluatorMock.Setup(x => x.EvaluateAsync(organization
                , projectId, buildDefinition, It.IsAny<IPipelineHasTaskRule>()))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithBuildDefinition_ShouldReturnFalse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        _pipelineEvaluatorMock.Setup(x => x.EvaluateAsync(organization
                , projectId, buildDefinition, It.IsAny<IPipelineHasTaskRule>()))
            .ReturnsAsync(false);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeFalse();
    }
}