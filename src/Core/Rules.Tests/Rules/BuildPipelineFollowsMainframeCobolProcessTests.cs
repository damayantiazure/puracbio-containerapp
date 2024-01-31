using AutoFixture;
using Moq;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class BuildPipelineFollowsMainframeCobolProcessTests
{
    private readonly BuildPipelineFollowsMainframeCobolProcess _sut;
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IPipelineEvaluatorFactory> _pipelineEvaluatorFactoryMock;
    private readonly Mock<IPipelineEvaluator> _pipelineEvaluatorMock;

    public BuildPipelineFollowsMainframeCobolProcessTests()
    {
        _pipelineEvaluatorFactoryMock = new Mock<IPipelineEvaluatorFactory>();
        _pipelineEvaluatorMock = new Mock<IPipelineEvaluator>();

        _pipelineEvaluatorFactoryMock.Setup(x => x.Create(It.IsAny<BuildDefinition>()))
            .Returns(_pipelineEvaluatorMock.Object);

        _sut = new BuildPipelineFollowsMainframeCobolProcess(null, _pipelineEvaluatorFactoryMock.Object);
    }

    [Fact]
    public async Task EvaluateAsync_WithInvalidOrganization_ShouldThrowArgumentNullException()
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
    public async Task EvaluateAsync_WithInvalidBuildValidation_ShouldThrowArgumentNullException()
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
    public async Task EvaluateAsync_ClassicBuildPipeline_ShouldBeTrue()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun)
            .Without(x => x.Yaml)
            .Create();

        _pipelineEvaluatorFactoryMock.Setup(x => x.EvaluateBuildTaskAsync(
                It.IsAny<IPipelineHasTaskRule>(), organization, projectId, buildDefinition))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_ClassicBuildPipelineWithOnlyOneTask_ShouldBeFalse()
    {
        // Arrange
        var taskId = TaskContants.MainframeCobolConstants.DbbBuildTaskId;
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.YamlUsedInRun)
            .Without(x => x.Yaml)
            .Create();

        _pipelineEvaluatorFactoryMock.Setup(x => x.EvaluateBuildTaskAsync(
                It.Is<IPipelineHasTaskRule>(x => x.TaskId == taskId)
                , organization, projectId, buildDefinition))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_YamlBuildPipeline_ShouldBeTrue()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.YamlUsedInRun, _fixture.Create<string>())
            .Create();

        _pipelineEvaluatorFactoryMock.Setup(x => x.EvaluateBuildTaskAsync(
                It.IsAny<IPipelineHasTaskRule>(), organization, projectId
                , buildDefinition))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_YamlBuildPipelineWithOnlyBuildTask_ShouldBeFalse()
    {
        // Arrange
        var taskId = TaskContants.MainframeCobolConstants.DbbBuildTaskId;
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.YamlUsedInRun, _fixture.Create<string>())
            .Create();

        _pipelineEvaluatorFactoryMock.Setup(x => x.EvaluateBuildTaskAsync(
                It.Is<IPipelineHasTaskRule>(x => x.TaskId == taskId)
                , organization, projectId, buildDefinition))
            .ReturnsAsync(true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        actual.ShouldBeFalse();
    }
}