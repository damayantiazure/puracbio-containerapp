using Rabobank.Compliancy.Infrastructure.Extensions;

namespace Rabobank.Compliancy.Infrastructure.Tests.Helpers;

public class ListExtensionsTests
{
    [Fact]
    public void Replace_ShouldReplaceOldItemWithNewItem()
    {
        // Arrange
        var a = new { Name = "a" };
        var b = new { Name = "b" };
        var c = new { Name = "c" };
        var d = new { Name = "d" };

        var list = new[] { a, b, c };

        // Act
        list.Replace(b, d);

        // Assert
        list[1].Name.Should().Be("d");
    }

    [Fact]
    public void Replace_ItemNotFound_ShouldThrowException()
    {
        // Arrange
        var a = new { Name = "a" };
        var b = new { Name = "b" };
        var c = new { Name = "c" };
        var d = new { Name = "d" };

        var list = new[] { a, b, c };

        // Act
        var func = () => list.Replace(d, b);

        // Assert
        func.Should().Throw<InvalidOperationException>();
    }
}