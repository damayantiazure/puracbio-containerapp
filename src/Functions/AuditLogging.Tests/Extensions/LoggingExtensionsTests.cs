#nullable enable

using Rabobank.Compliancy.Functions.AuditLogging.Extensions;
using Shouldly;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Extensions;

public class LoggingExtensionsTests
{
    [Theory]
    [InlineData("2023-02-03T07:37:43.8469682Z Generating script.", "Generating script.")]
    [InlineData("2022-12-03T07:37:43.8469682Z  Generating script.", " Generating script.")]
    [InlineData("Test 2022-12-03T07:37:43.8469682Z Generating script.", "Test Generating script.")]
    public void RemoveUniversalDataTimeString_CorrectFormats_ReturnsStringWithRemovedDataTime(string logString, string expectedResult)
    {
        // Act
        var resultString = logString.RemoveUniversalDateTimeString();

        // Assert
        resultString.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData("202-02-03T07:37:43.8469682Z Generating script.")]
    [InlineData("2022-1-03T07:37:43.8469682Z  Generating script.")]
    [InlineData("2022-12-03A07:37:43.8469682Z Generating script.")]
    [InlineData("2022-12-03T07:37:43.8469682 Generating script.")]
    [InlineData("2022-12-03T07:37:43.846682 Generating script.")]
    public void RemoveUniversalDataTimeString_InCorrectFormats_ReturnsOriginalString(string logString)
    {
        // Act
        var resultString = logString.RemoveUniversalDateTimeString();

        // Assert
        resultString.ShouldBe(logString);
    }

    [Theory]
    [InlineData("logline1 \r\nlogline2 \r\nlogline3", "logline1 logline2 logline3")]
    [InlineData("logline1logline2logline3", "logline1logline2logline3")]
    public void RemoveNewlines_StringsWithAndWithoutNewLines_ReturnsExpectedString(string logString, string expectedString)
    {
        // Act
        string resultString = logString.RemoveNewlines();

        // Assert
        resultString.ShouldBe(expectedString);
    }
}