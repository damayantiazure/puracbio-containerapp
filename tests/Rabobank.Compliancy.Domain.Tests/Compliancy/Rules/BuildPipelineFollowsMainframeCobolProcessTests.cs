using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class BuildPipelineFollowsMainframeCobolProcessTests : PipelineHasTaskRuleTestsBase
{
    private readonly BuildPipelineFollowsMainframeCobolProcess _sut;
    private const string DbbBuildTaskId = "f0ed76ac-b927-42fa-a758-a36c1838a13b";
    private const string DbbPackageTaskId = "dc5c403b-4cd3-48f2-9dcc-4405e1b6f981";

    public BuildPipelineFollowsMainframeCobolProcessTests()
    {
        _sut = new BuildPipelineFollowsMainframeCobolProcess();
    }

    [Theory]
    [InlineData("dbb-build", "dbb-package")]
    [InlineData(DbbBuildTaskId, DbbPackageTaskId)]
    public void Evaluate_PipelineWithDbbBuildAndDbbPackageTask_ReturnsTrueResult(string task1, string task2)
    {
        // Arrange
        var pipeline = new Pipeline();
        var dbbBuildTask = CreateTask(task1);
        var dbbPackageTask = CreateTask(task2);

        pipeline.DefaultRunContent = new PipelineBody
        {
            Tasks = new List<PipelineTask> { dbbBuildTask, dbbPackageTask }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Theory]
    [InlineData("dbb-build")]
    [InlineData("dbb-package")]
    public void Evaluate_PipelineWithoutBothTasksTogether_ReturnsFalseResult(string taskName)
    {
        // Arrange
        var pipeline = new Pipeline();
        var dbbBuildTask = CreateTask(taskName);

        pipeline.DefaultRunContent = new PipelineBody
        {
            Tasks = new List<PipelineTask> { dbbBuildTask, dbbBuildTask }
        };

        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }
}