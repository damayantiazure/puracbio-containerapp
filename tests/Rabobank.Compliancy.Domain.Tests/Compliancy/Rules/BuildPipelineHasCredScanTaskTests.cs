using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildPipelineHasCredScanTaskTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildPipelineHasCredScanTask _sut;
    private const string CredScanTaskId = "f0462eae-4df1-45e9-a754-8184da95ed01";
    private const string PostAnalysisTaskId = "dbe519ee-a2e4-43f5-8e1a-949bd935b736";

    public BuildPipelineHasCredScanTaskTests()
    {
        _sut = new BuildPipelineHasCredScanTask();
    }

    [Theory]
    [InlineData("CredScan", "PostAnalysis")]
    [InlineData("credscan", "postanalysis")]
    [InlineData(CredScanTaskId, PostAnalysisTaskId)]
    public void Evaluate_BuildPipelineHasCredScanTaskWithCredTask_ReturnsTrueResult(string credScanTask, string postAnalysisTask)
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(credScanTask),
                    CreateTask(postAnalysisTask, new Dictionary<string, string> { { "credscan", "true" } }),
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

    [Theory]
    [InlineData("CredScan")]
    [InlineData("PostAnalysis")]
    public void Evaluate_BuildPipelineOnlyOneOftheExpectedTasksPresent_ReturnsFalseResult(string taskName)
    {
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(taskName),
                    CreateTask("DummyTask")
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