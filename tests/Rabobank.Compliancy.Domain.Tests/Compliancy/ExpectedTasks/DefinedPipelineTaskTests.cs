using FluentAssertions;
using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.ExpectedTasks;

public class DefinedPipelineTaskTests
{
    [Fact]
    public void Initializing_WithNullString_ThrowsArgumentException()
    {
        // Arrange
        string name = null;

        // Act
        var act = () => new DefinedPipelineTask(Guid.NewGuid(), name);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Initializing_WithEmptyString_ThrowsArgumentlException()
    {
        // Arrange
        string name = "";

        // Act
        var act = () => new DefinedPipelineTask(Guid.NewGuid(), name);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Initializing_WithWhitespaceString_ThrowsArgumentException()
    {
        // Arrange
        string name = "   ";

        // Act
        var act = () => new DefinedPipelineTask(Guid.NewGuid(), name);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}