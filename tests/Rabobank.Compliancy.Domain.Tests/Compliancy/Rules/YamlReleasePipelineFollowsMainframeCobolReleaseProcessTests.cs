using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class YamlReleasePipelineFollowsMainframeCobolReleaseProcessTests : PipelineHasTaskRuleTestsBase
{
    private readonly YamlReleasePipelineFollowsMainframeCobolReleaseProcess _sut;

    public YamlReleasePipelineFollowsMainframeCobolReleaseProcessTests()
    {
        _sut = new YamlReleasePipelineFollowsMainframeCobolReleaseProcess();
    }

    [Theory]
    [InlineData("206089fc-dcf1-4d0a-bc10-135adf95db3c")]
    [InlineData("dbb-deploy-prod")]
    public void Evaluate_PipelineWithDbbDeployTaskTaskNameAsGuidAndAllInputs_ReturnsTrueResult(string taskName)
    {
        // Arrange
        var pipeline = new Pipeline();
        var dbbDeployTask = CreateTask(taskName, new Dictionary<string, string> {
            { "OrganizationName", "unittest" },
            { "ProjectId", "dummy" },
            { "PipelineId", "dummy2" }
        });

        pipeline.DefaultRunContent = new PipelineBody
        {
            Tasks = new List<PipelineTask> { dbbDeployTask }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_PipelineWithDbbDeployTaskNotAllInputs_ReturnsFalseResult()
    {
        // Arrange
        var pipeline = new Pipeline()
        {
            DefaultRunContent = new PipelineBody
            {
                Tasks = new[] {
                    CreateTask("dbb-deploy-prod",
                        new Dictionary<string, string> {
                            { "OrganizationName", "dummy" },
                            { "ProjectId", "dummy2" }
                        }
                    )
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