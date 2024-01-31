using AutoFixture;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class ClassicReleasePipelineFollowsMainframeCobolReleaseProcessTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ClassicReleasePipelineFollowsMainframeCobolReleaseProcess _sut;

    public ClassicReleasePipelineFollowsMainframeCobolReleaseProcessTests()
    {
        _sut = new ClassicReleasePipelineFollowsMainframeCobolReleaseProcess(null);
    }

    [Fact]
    public async Task EvaluateAsync_WithNoReleaseDefinition_ShouldThrowArgumentNullException()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        ReleaseDefinition buildDefinition = null;

        // Act
        Func<Task<bool>> actual = () => _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.ShouldThrowAsync<ArgumentNullException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoReleaseDefinitionEnvironments_ShouldThrowArgumentOutOfRangeException()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        var buildDefinition = _fixture.Build<ReleaseDefinition>()
            .Without(x => x.Environments).Create();

        // Act
        Func<Task<bool>> actual = () => _sut
            .EvaluateAsync(organization, projectId, buildDefinition);

        // Assert
        await actual.ShouldThrowAsync<ArgumentOutOfRangeException>();
    }

    [Fact]
    public async Task EvaluateAsync_WithNoWorkFlowTasks_ShouldReturnFalse()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        var releaseDefinition = CreateReleaseDefinition(null);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, releaseDefinition);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithWorkFlowTasksAndValidProperties_ShouldReturnTrue()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        var inputs = new Dictionary<string, string>
        {
            { TaskContants.MainframeCobolConstants.OrganizationName, _fixture.Create<string>() },
            { TaskContants.MainframeCobolConstants.ProjectId,  _fixture.Create<string>() },
            { TaskContants.MainframeCobolConstants.PipelineId, _fixture.Create<string>() }
        };

        var releaseDefinition = CreateReleaseDefinition(inputs, true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, releaseDefinition);

        // Assert
        actual.ShouldBeTrue();
    }

    [Fact]
    public async Task EvaluateAsync_WithWorkFlowTasksAndInvalidProperties_ShouldReturnFalse()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        var inputs = new Dictionary<string, string>
        {
            { _fixture.Create<string>(), _fixture.Create<string>() },
            { _fixture.Create<string>(), _fixture.Create<string>() },
            { _fixture.Create<string>(), _fixture.Create<string>() }
        };

        var releaseDefinition = CreateReleaseDefinition(inputs, true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, releaseDefinition);

        // Assert
        actual.ShouldBeFalse();
    }

    [Fact]
    public async Task EvaluateAsync_WithWorkFlowTasksAndNullValueProperties_ShouldReturnFalse()
    {
        // Arrange
        var organization = string.Empty;
        var projectId = string.Empty;
        var inputs = new Dictionary<string, string>
        {
            { TaskContants.MainframeCobolConstants.OrganizationName, string.Empty },
            { TaskContants.MainframeCobolConstants.ProjectId,  string.Empty },
            { TaskContants.MainframeCobolConstants.PipelineId, string.Empty }
        };

        var releaseDefinition = CreateReleaseDefinition(inputs, true);

        // Act
        var actual = await _sut
            .EvaluateAsync(organization, projectId, releaseDefinition);

        // Assert
        actual.ShouldBeFalse();
    }

    private ReleaseDefinition CreateReleaseDefinition(Dictionary<string, string> inputs, bool hasDeployTask = false)
    {
        var taskId = hasDeployTask
            ? new Guid(TaskContants.MainframeCobolConstants.DbbDeployTaskId)
            : Guid.NewGuid();

        var workFlowTasks = _fixture.Build<WorkflowTask>()
            .With(x => x.TaskId, taskId)
            .With(x => x.Enabled, true)
            .With(x => x.Inputs, inputs).CreateMany(1).ToList();

        var deployPhases = _fixture.Build<DeployPhase>()
            .With(x => x.WorkflowTasks, workFlowTasks).CreateMany(1).ToList();

        var environments = _fixture.Build<ReleaseDefinitionEnvironment>()
            .With(x => x.DeployPhases, deployPhases).CreateMany(1).ToList();

        var releaseDefinition = _fixture.Build<ReleaseDefinition>()
            .With(x => x.Environments, environments).Create();

        return releaseDefinition;
    }
}