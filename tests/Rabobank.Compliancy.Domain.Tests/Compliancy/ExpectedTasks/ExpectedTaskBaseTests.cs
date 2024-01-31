using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
using Rabobank.Compliancy.Domain.Tests.Compliancy.ExpectedTasks.Implementations;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.ExpectedTasks;

public class ExpectedTaskBaseTests
{
    private readonly ExpectedTaskBase _sut;
    private readonly ExpectedTaskImplementation expectedTaskImplementation;

    public ExpectedTaskBaseTests()
    {
        expectedTaskImplementation = new ExpectedTaskImplementation();
        _sut = expectedTaskImplementation;
    }

    [Fact]
    public void IsSameTask_WithNullInput_ShouldReturnFalse()
    {
        // Arrange
        PipelineTask pipelineTask = null;

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameTask_WithEmptyPipelineTask_ShouldReturnFalse()
    {
        // Arrange
        var pipelineTask = new PipelineTask();

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameTask_WithCorrectTaskId_ShouldReturnTrue()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Id = expectedTaskImplementation.GettableTaskId
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameTask_WithCorrectTaskIdAsName_ShouldReturnTrue()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Name = expectedTaskImplementation.GettableTaskId.ToString()
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameTask_WithCorrectTaskName_ShouldReturnTrue()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Name = expectedTaskImplementation.GettableTaskName
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSameTask_WithInCorrectTaskId_ShouldReturnFalse()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Id = Guid.NewGuid()
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameTask_WithInCorrectTaskName_ShouldReturnFalse()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Name = Guid.NewGuid().ToString()
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameTask_WithCorrectTaskName_ButIncorrectTaskId_ShouldReturnFalse()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Name = expectedTaskImplementation.GettableTaskName,
            Id = Guid.NewGuid()
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSameTask_WithCorrectTaskName_AndCorrectTaskId_ShouldReturnTrue()
    {
        // Arrange
        var pipelineTask = new PipelineTask()
        {
            Name = expectedTaskImplementation.GettableTaskName,
            Id = expectedTaskImplementation.GettableTaskId
        };

        // Act
        var result = _sut.IsSameTask(pipelineTask);

        // Assert
        result.Should().BeTrue();
    }
}