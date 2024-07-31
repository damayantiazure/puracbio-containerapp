#nullable enable

using System.Collections;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.Services;

public interface ILoggingService
{
    public Task LogInformationAsync(LogDestinations logDestination, object input);
    public Task LogInformationItemsAsync(LogDestinations logDestination, IEnumerable input);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionReport exceptionReport);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, Exception e);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, Exception e, string? ciIdentifier);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, string? ciIdentifier,
        ExceptionSummaryReport exception);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, Exception e, User? user,
        string? pipelineId, string? stageId, string? ciOrPipelineIdentifier);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, string? itemId, string? ruleName, Exception e);

    public Task LogExceptionAsync(LogDestinations logDestination,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, Exception e, string? itemId, string? ruleName,
        string? ciIdentifier);

    public Task LogExceptionAsync(LogDestinations logDestination, Exception e,
        ExceptionBaseMetaInformation exceptionBaseMetaInformation, string? itemId, string? itemType);
}