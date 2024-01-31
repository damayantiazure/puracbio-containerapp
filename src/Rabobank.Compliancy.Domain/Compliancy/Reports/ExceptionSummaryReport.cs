#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class ExceptionSummaryReport
{
    public ExceptionSummaryReport(string exceptionType, string exceptionMessage, string? innerExceptionType,
        string? innerExceptionMessage)
    {
        ExceptionType = exceptionType;
        ExceptionMessage = exceptionMessage;
        InnerExceptionType = innerExceptionType;
        InnerExceptionMessage = innerExceptionMessage;
    }

    public ExceptionSummaryReport(Exception exception) :
        this(exception.GetType().Name,
            $"{exception.Message} Stacktrace: {exception.StackTrace}",
            exception.InnerException?.GetType().Name,
            exception.InnerException?.Message)
    {
    }

    public string ExceptionType { get; }
    public string ExceptionMessage { get; }
    public string? InnerExceptionType { get; }
    public string? InnerExceptionMessage { get; }
}