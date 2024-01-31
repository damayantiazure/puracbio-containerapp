using FluentAssertions;
using Rabobank.Compliancy.Domain.Builders;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Tests;

public class DefinedTaskBuilderTests
{
    [Fact]
    public void Constructor_WithNameAndOrIdEmptyOrNull_ThrowsArgumentException()
    {
        //Arrange
        var expectedError = "Task ID and task name cannot be null or empty.";
        // Act / Assert
        var action = () => new DefinedTaskBuilder(Guid.Empty, null);
        action.Should().Throw<ArgumentException>().WithMessage(expectedError);

        action = () => new DefinedTaskBuilder(Guid.Empty, string.Empty);
        action.Should().Throw<ArgumentException>().WithMessage(expectedError);

        action = () => new DefinedTaskBuilder(Guid.Empty, "nameHasValue");
        action.Should().Throw<ArgumentException>().WithMessage(expectedError);

        action = () => new DefinedTaskBuilder(Guid.NewGuid(), string.Empty);
        action.Should().Throw<ArgumentException>().WithMessage(expectedError);

        action = () => new DefinedTaskBuilder(Guid.NewGuid(), null);
        action.Should().Throw<ArgumentException>().WithMessage(expectedError);
    }

    [Fact]
    public void Build_TaskWithNameAndId_ReturnsTask()
    {
        // Arrange
        var taskId = Guid.NewGuid();
        const string taskName = "testtask";

        // Act
        var task = new DefinedTaskBuilder(taskId, taskName).Build();

        // Assert
        task.Id.Should().Be(taskId);
        task.Name.Should().Be(taskName);
    }

    [Fact]
    public void Build_TaskWithInputs_ReturnsTaskWithInputs()
    {
        // Arrange
        const string key1 = "key1";
        const string key2 = "key2";
        var input1 = new ExpectedInputValue("unittest");
        var input2 = new ExpectedInputValue("unittest2");

        // Act
        var task = new DefinedTaskBuilder(Guid.NewGuid(), "taskname")
            .WithSpecificValueInput(key1, input1)
            .WithSpecificValueInput(key2, input2)
            .Build();

        // Assert
        task.Inputs.Count.Should().Be(2);
        task.Inputs[key1].Should().Be(input1);
        task.Inputs[key2].Should().Be(input2);
    }
}