using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildArtifactIsStoredSecureTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildArtifactIsStoredSecure _sut;
    private const string PublishBuildArtifactsTaskId = "2ff763a7-ce83-4e1f-bc89-0ae63477cebe";
    private const string PublishPipelineArtifactTaskId = "ecdc45f6-832d-4ad9-b52b-ee49e94659be";

    public BuildArtifactIsStoredSecureTests()
    {
        _sut = new BuildArtifactIsStoredSecure();
    }

    [Theory]
    [InlineData("PublishBuildArtifacts")]
    [InlineData("PublishPipelineArtifact")]
    [InlineData(PublishBuildArtifactsTaskId)]
    [InlineData(PublishPipelineArtifactTaskId)]
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