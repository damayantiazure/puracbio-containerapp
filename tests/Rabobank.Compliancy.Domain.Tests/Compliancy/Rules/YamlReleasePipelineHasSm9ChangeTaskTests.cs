using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class YamlReleasePipelineHasSm9ChangeTaskTests : PipelineHasTaskRuleTestsBase
{
    private readonly YamlReleasePipelineHasSm9ChangeTask _sut;

    public YamlReleasePipelineHasSm9ChangeTaskTests()
    {
        _sut = new YamlReleasePipelineHasSm9ChangeTask();
    }

    [Theory]
    [InlineData("SM9 - Create", "SM9 - Approve")]
    [InlineData("d0c045b6-d01d-4d69-882a-c21b18a35472", "73cb0c6a-0623-4814-8774-57dc1ef33858")]
    public void Evaluate_YamlReleasePipelineHasSm9ChangeTaskWithExpectedTaskOrId_ReturnsTrueResult(string createTask, string approveTask)
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(createTask),
                    CreateTask("DummyTask"),
                    CreateTask(approveTask)
                }
            }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Theory]
    [InlineData("SM9 - Create")]
    [InlineData("SM9 - Approve")]
    [InlineData("d0c045b6-d01d-4d69-882a-c21b18a35472")]
    [InlineData("73cb0c6a-0623-4814-8774-57dc1ef33858")]
    public void Evaluate_YamlReleasePipelineHasSm9ChangeTaskWithOnlyOneExpectedTaskOrId_ReturnsTrueResult(string task)
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask("DummyTask"),
                    CreateTask(task)
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
    public void Evaluate_BuildArtifactIsStoredSecureWithoutExpectedTasks_ReturnsFalseResult()
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