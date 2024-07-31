#nullable enable

using System.Collections;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Task = System.Threading.Tasks.Task;
using User = Rabobank.Compliancy.Domain.Compliancy.Authorizations.User;

namespace Rabobank.Compliancy.Infrastructure;

internal class LoggingService : ILoggingService
{
    private readonly ILogIngestionService _logIngestionService;

    public LoggingService(ILogIngestionService logIngestionService) => 
        _logIngestionService = logIngestionService;

    public Task LogInformationAsync(LogDestinations logDestination, object input) => 
        _logIngestionService.WriteLogEntryAsync(input, logDestination);

    public Task LogInformationItemsAsync(LogDestinations logDestination, IEnumerable input) =>
        _logIngestionService.WriteLogEntriesAsync(input, logDestination);

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionReport exceptionReport) =>
        LogExceptionReportAsync(logDestination, exceptionReport);

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        Exception e)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e);
        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        Exception e, string? ciIdentifier)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e)
        {
            CiIdentifier = ciIdentifier
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        string? ciIdentifier, ExceptionSummaryReport exception)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation)
        {
            ExceptionType = exception.ExceptionType,
            ExceptionMessage = exception.ExceptionMessage,
            InnerExceptionType = exception.InnerExceptionType,
            InnerExceptionMessage = exception.InnerExceptionMessage,
            CiIdentifier = ciIdentifier
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        Exception e, User? user, string? pipelineId, string? stageId, string? ciOrPipelineIdentifier)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e)
        {
            ItemId = pipelineId,
            StageId = stageId,
            CiIdentifier = ciOrPipelineIdentifier,
            UserId = user == null ? Guid.Empty : Guid.Parse(user.UniqueId),
            UserMail = user?.MailAddress
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        string? itemId, string? ruleName, Exception e)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e)
        {
            ItemId = itemId,
            RuleName = ruleName
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        Exception e, string? itemId, string? ruleName, string? ciIdentifier)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e)
        {
            ItemId = itemId,
            RuleName = ruleName,
            CiIdentifier = ciIdentifier
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    public Task LogExceptionAsync(LogDestinations logDestination, Exception e, ExceptionBaseMetaInformation exceptionBaseMetaInformation,
        string? itemId, string? itemType)
    {
        var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, e)
        {
            ItemId = itemId,
            ItemType = itemType
        };

        return LogExceptionReportAsync(logDestination, exceptionReport);
    }

    private Task LogExceptionReportAsync(LogDestinations logDestination, ExceptionReport exceptionReport) => 
        _logIngestionService.WriteLogEntryAsync(exceptionReport, logDestination);
}