using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildPipelineHasSonarQubeTaskTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildPipelineHasSonarQubeTask _sut;

    public BuildPipelineHasSonarQubeTaskTests()
    {
        _sut = new BuildPipelineHasSonarQubeTask();
    }

    [Theory]
    [InlineData("GuiMavenTask", "sqAnalysisEnabled", "true")]
    [InlineData("Maven", "sonarQubeRunAnalysis", "true")] // no ID for this task in the old code
    [InlineData("SonarQubeAnalyze", null, null)]
    [InlineData("ac4ee482-65da-4485-a532-7b085873e532", "sqAnalysisEnabled", "true")] // GuiMavenTask
    [InlineData("15b84ca1-b62f-4a2a-a403-89b77a063157", "sonarQubeRunAnalysis", "true")] // SonarQubeAnalyze
    public void Evaluate_BuildArtifactIsStoredSecureWithExpectedTaskOrId_ReturnsTrueResult(string task, string inputKey, string inputValue)
    {
        var inputs = string.IsNullOrEmpty(inputKey) ? null : new Dictionary<string, string> { { inputKey, inputValue } };
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(task, inputs),
                    CreateTask("DummyTask"),
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
    [InlineData("GuiMavenTask", "sqAnalysisEnabled", "false")]
    [InlineData("GuiMavenTask", "test", "true")]
    [InlineData("GuiMavenTask", null, null)]
    [InlineData("Maven", "sonarQubeRunAnalysis", "false")] // no ID for this task in the old code
    [InlineData("Maven", "test", "true")]
    [InlineData("Maven", null, null)]
    public void Evaluate_BuildArtifactIsStoredSecureWithIncorrectInput_ReturnsFalseResult(string task, string inputKey, string inputValue)
    {
        var inputs = string.IsNullOrEmpty(inputKey) ? null : new Dictionary<string, string> { { inputKey, inputValue } };
        // Arrange 
        var pipeline = new Pipeline
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new List<PipelineTask>
                {
                    CreateTask(task, inputs),
                    CreateTask("DummyTask"),
                }
            }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
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