using MemoryCache.Testing.Moq;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Helpers;

public class ClassicPipelineEvaluatorTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ClassicPipelineEvaluator _sut;
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();

    public ClassicPipelineEvaluatorTests()
    {
        var memoryCache = Create.MockedMemoryCache();
        _sut = new ClassicPipelineEvaluator(_azdoRestClientMock.Object, memoryCache);
    }

    [Fact]
    public void EvaluateAsync_WithNoOrganization_ShouldThrowArgumentNullException()
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(null, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsync_WithNoProjectId_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, null, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsync_WithNoBuildDefinition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, null, pipelineHasTaskRule);

        // Assert
        actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public void EvaluateAsync_WithNoBuildDefinitionProperties_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();

        var buildProcess = _fixture.Build<BuildProcess>()
            .Without(x => x.Phases).Create();
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .With(x => x.Process, buildProcess).Create();
        var pipelineHasTaskRule = _fixture.Create<PipelineHasTaskRule>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().ThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void EvaluateAsync_WithNoPipelineHasTaskRule_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var buildDefinition = _fixture.Create<BuildDefinition>();

        // Act
        Func<Task<bool>> actual = () => _sut.EvaluateAsync(organization, projectId, buildDefinition, null);

        // Assert
        actual.Should().ThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task GetPipelinesAsync_WithNoBuildDefinitioneProcess_ShouldReturnEmptyCollection()
    {
        // Arrange
        var buildDefinition = _fixture.Build<BuildDefinition>()
            .Without(x => x.Process).Create();

        // Act
        var actual = await _sut.GetPipelinesAsync(null, null, null, buildDefinition, null);

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoEnabledPipelineTask_ShouldValidateAndReturnFalse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var taskId = _fixture.Create<string>();

        // assign the rule task id to the build task
        var buildTask = _fixture.Build<BuildTask>().Without(x => x.Id).Create();
        var buildSteps = _fixture.Build<BuildStep>()
            .With(x => x.Enabled, false).With(x => x.Task, buildTask).CreateMany();

        var buildPhases = _fixture.Build<BuildPhase>().With(x => x.Steps, buildSteps).CreateMany();
        var buildDefinition = _fixture.Build<BuildDefinition>().Create();
        buildDefinition.Process.Phases = buildPhases;

        // assign the rule with a task id and inputs that needs to be matched
        var pipelineHasTaskRule = _fixture.Build<PipelineHasTaskRule>().With(x => x.TaskId, taskId).Create();

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithMultiplePipelineTasks_ThatDoesNotContainValidInputs_ShouldValidateAndReturnFalse()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var taskId = _fixture.Create<string>();
        var inputs = _fixture.Create<Dictionary<string, string>>();

        // assign the rule task id to the build task
        var buildTask = _fixture.Build<BuildTask>().With(x => x.Id, taskId).Create();
        var buildSteps = _fixture.Build<BuildStep>()
            .With(x => x.Enabled, true).With(x => x.Task, buildTask).CreateMany().ToList();

        var buildPhases = _fixture.Build<BuildPhase>().With(x => x.Steps, buildSteps).CreateMany();
        var buildDefinition = _fixture.Build<BuildDefinition>().Create();
        buildDefinition.Process.Phases = buildPhases;

        // assign the rule with a task id and inputs that needs to be matched
        var pipelineHasTaskRule = _fixture.Build<PipelineHasTaskRule>().With(x => x.TaskId, taskId)
            .With(x => x.Inputs, inputs).Create();

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithMultiplePipelineTasks_ThatContainsOneTaskWithValidInputs_ShouldValidateAndReturnTrue()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var taskId = _fixture.Create<string>();
        var inputs = _fixture.Create<Dictionary<string, string>>();

        // assign the rule task id to the build task
        var buildTask = _fixture.Build<BuildTask>().With(x => x.Id, taskId).Create();
        var buildSteps = _fixture.Build<BuildStep>()
            .With(x => x.Enabled, true).With(x => x.Task, buildTask).CreateMany().ToList();

        // set only one build step to enabled with the matching rule inputs
        buildSteps[2].Inputs = inputs;

        var buildPhases = _fixture.Build<BuildPhase>().With(x => x.Steps, buildSteps).CreateMany();
        var buildDefinition = _fixture.Build<BuildDefinition>().Create();
        buildDefinition.Process.Phases = buildPhases;

        // assign the rule with a task id and inputs that needs to be matched
        var pipelineHasTaskRule = _fixture.Build<PipelineHasTaskRule>().With(x => x.TaskId, taskId)
            .With(x => x.Inputs, inputs).Create();

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithEnabledTaskAndNoRequiredRuleInputs_ShouldValidateAndReturnTrue()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var projectId = _fixture.Create<Guid>().ToString();
        var taskId = _fixture.Create<string>();

        // assign the rule task id to the build task
        var buildTask = _fixture.Build<BuildTask>().With(x => x.Id, taskId).Create();
        var buildSteps = _fixture.Build<BuildStep>().With(x => x.Task, buildTask).CreateMany().ToList();

        // set only one build step to enabled with the matching rule inputs
        buildSteps[0].Enabled = true;

        var buildPhases = _fixture.Build<BuildPhase>().With(x => x.Steps, buildSteps).CreateMany().ToList();
        var buildDefinition = _fixture.Build<BuildDefinition>().Create();
        buildDefinition.Process.Phases = buildPhases;

        // assign the rule with a task id and inputs that needs to be matched
        var pipelineHasTaskRule = _fixture.Build<PipelineHasTaskRule>().With(x => x.TaskId, taskId)
            .Without(x => x.Inputs).Create();

        // Act
        var actual = await _sut.EvaluateAsync(organization, projectId, buildDefinition, pipelineHasTaskRule);

        // Assert
        actual.Should().BeTrue();
    }
}