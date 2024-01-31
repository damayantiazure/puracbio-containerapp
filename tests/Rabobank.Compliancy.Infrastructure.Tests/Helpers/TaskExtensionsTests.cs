using Rabobank.Compliancy.Infrastructure.Extensions;

namespace Rabobank.Compliancy.Infrastructure.Tests.Helpers;

public class TaskExtensionsTests
{
    [Fact]
    public void ToParsedTaskname_TasknameNull_ReturnsEmptyString()
    {
        // Arrange
        string? fullTaskname = null;

        // Act
        string parsedTaskName = fullTaskname.StripNamespaceAndVersion();

        // Assert
        parsedTaskName.Should().NotBeNull();
    }

    [Theory]
    [InlineData("namespace.unittest.myTask")]
    [InlineData("unittest.myTask")]
    [InlineData("unittest.myTask@3")]
    [InlineData("myTask@3")]
    [InlineData("myTask")]
    public void ToParsedTaskname_TasknameDifferntFormats_ReturnsTaskname(string fullTaskname)
    {
        // Act
        string parsedTaskName = fullTaskname.StripNamespaceAndVersion();

        // Assert
        parsedTaskName.Should().Be("myTask");
    }
}