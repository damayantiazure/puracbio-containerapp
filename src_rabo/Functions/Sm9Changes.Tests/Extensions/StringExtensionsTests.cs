#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("", false)]
    [InlineData("123456789", false)]
    [InlineData("1ab456789", false)]
    [InlineData("C123456789", true)]
    public void ValidatingChangeId_ReturnsExpectedResult(string changeId, bool expectedResult)
    {
        // Act
        var result = changeId.IsValidChangeId();

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("", false)]
    [InlineData("1ab456789", false)]
    [InlineData("123456", true)]
    public void ValidatingBuildReleaseId_ReturnsExpectedResult(string releaseId, bool expectedResult)
    {
        // Act
        var result = releaseId.IsValidPipelineRunId();

        // Assert
        result.ShouldBe(expectedResult);
    }
}