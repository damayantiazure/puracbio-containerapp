using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildPipelineHasNexusIqTaskTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildPipelineHasNexusIqTask _sut;

    public BuildPipelineHasNexusIqTaskTests()
    {
        _sut = new BuildPipelineHasNexusIqTask();
    }

    [Theory]
    [InlineData("NexusIqPipelineTask")]
    [InlineData("4f40d1a2-83b0-4ddc-9a77-e7f279eb1802")]
    public void Evaluate_BuildArtifactIsStoredSecureWithExpectedTaskOrId_ReturnsTrueResult(string task)
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(task),
                    CreateTask("DummyTask")
                }
            }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_BuildArtifactIsStoredSecureWithoutPublishOrPipelineTask_ReturnsFalseResult()
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask("DummyTask"),
                    CreateTask("DummyTask2")
                }
            }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }
}