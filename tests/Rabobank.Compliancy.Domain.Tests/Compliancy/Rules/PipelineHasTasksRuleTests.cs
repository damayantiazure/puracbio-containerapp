using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Compliancy.Rules;
using Rabobank.Compliancy.Domain.Tests.Compliancy.ExpectedTasks.Implementations;
using Rabobank.Compliancy.Domain.Tests.Compliancy.Rules.TestImplementations;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class PipelineHasTasksRuleTests
{
    private readonly PipelineHasTasksRule _sut;

    private readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask>()
    {
        new ExpectedTaskImplementation()
    };

    public PipelineHasTasksRuleTests()
    {
        _sut = new PipelineHasTasksRuleImplementation(_expectedTasks);
    }

    [Fact]
    public void Evaluate_WhenEvaluatableHasNoTasks_ShouldReturnFalseResult()
    {
        // Arrange 
        var pipeline = new Pipeline();
        var evaluatable = new TaskContainingEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(evaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void PipelineHasTasksRule_WhenConstructedWithNullInput_ShouldThrow()
    {
        // Act
        var act = () => new PipelineHasTasksRuleImplementation(null);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void PipelineHasTasksRule_WhenConstructedWithEmptyEnumerable_ShouldThrow()
    {
        // Arrange 
        var expectedTasks = new List<IExpectedTask>();

        // Act
        var act = () => new PipelineHasTasksRuleImplementation(expectedTasks);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}