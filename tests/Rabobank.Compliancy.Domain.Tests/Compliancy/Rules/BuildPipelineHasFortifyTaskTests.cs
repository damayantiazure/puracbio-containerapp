using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildPipelineHasFortifyTaskTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildPipelineHasFortifyTask _sut;
    private const string FortifyTaskId = "818386e5-c8a5-46c3-822d-954b3c8fb130";

    public BuildPipelineHasFortifyTaskTests()
    {
        _sut = new BuildPipelineHasFortifyTask();
    }

    [Theory]
    [InlineData("FortifySCA")]
    [InlineData("fortifysca")]
    [InlineData(FortifyTaskId)]
    public void Evaluate_BuildPipelineHasFortifyTaskWithExpectedTaskOrId_ReturnsTrueResult(string task)
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
    public void Evaluate_BuildPipelineHasFortifyTaskWithoutFortifyTask_ReturnsFalseResult()
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