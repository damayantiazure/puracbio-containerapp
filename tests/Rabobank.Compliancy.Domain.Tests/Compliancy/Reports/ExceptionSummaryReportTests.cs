using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class ExceptionSummaryReportTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Ctor_WithExceptionArgument_ShouldPopulateSummary()
    {
        // Arrange
        var innerMessage = _fixture.Create<string>();
        var inner = new InvalidOperationException(innerMessage);

        var message = _fixture.Create<string>();
        var exception = new Exception(message, inner);

        // Act
        var actual = new ExceptionSummaryReport(exception);

        // Assert
        actual.ExceptionType.Should().Be("Exception");
        actual.ExceptionMessage.Should().Contain(message);
        actual.InnerExceptionType.Should().Be("InvalidOperationException");
        actual.InnerExceptionMessage.Should().Be(innerMessage);
    }

    [Fact]
    public void Ctor_WithStringArguments_ShouldPopulateSummary()
    {
        // Arrange
        var exceptionType = _fixture.Create<string>();
        var exceptionMessage = _fixture.Create<string>();
        var innerExceptionType = _fixture.Create<string>();
        var innerExceptionMessage = _fixture.Create<string>();

        // Act
        var actual = new ExceptionSummaryReport(exceptionType, exceptionMessage, innerExceptionType, innerExceptionMessage);

        // Assert
        actual.ExceptionType.Should().Be(exceptionType);
        actual.ExceptionMessage.Should().Contain(exceptionMessage);
        actual.InnerExceptionType.Should().Be(innerExceptionType);
        actual.InnerExceptionMessage.Should().Be(innerExceptionMessage);
    }
}