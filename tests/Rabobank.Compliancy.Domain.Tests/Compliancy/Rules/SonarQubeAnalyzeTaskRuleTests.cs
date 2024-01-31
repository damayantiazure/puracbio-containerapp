using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Compliancy.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class SonarQubeAnalyzeTaskRuleTests : PipelineHasTaskRuleTestsBase
{
    private readonly PipelineHasSonarQubeAnalyzeTaskRule _sut;

    public SonarQubeAnalyzeTaskRuleTests()
    {
        _sut = new PipelineHasSonarQubeAnalyzeTaskRule();
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasTaskWithCorrectId_ReturnsTrueResult()
    {
        // Arrange
        var pipeline = new Pipeline()
        {
            DefaultRunContent = new PipelineBody()
            {
                Tasks = new List<PipelineTask>() {
                    CreateTask(new Guid("15b84ca1-b62f-4a2a-a403-89b77a063157"))
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
    public void Evaluate_WhenEvaluatableHasTaskWithCorrectName_ReturnsTrueResult()
    {
        // Arrange
        var pipeline = new Pipeline()
        {
            DefaultRunContent = new PipelineBody()
            {
                Tasks = new List<PipelineTask>() {
                    CreateTask("SonarQubeAnalyze")
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
    public void Evaluate_WhenEvaluatableHasNoTasks_ReturnsFalseResult()
    {
        // Arrange 
        var pipeline = new Pipeline();
        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }
}