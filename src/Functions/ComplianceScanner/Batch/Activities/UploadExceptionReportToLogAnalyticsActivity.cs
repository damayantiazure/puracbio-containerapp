#nullable enable

using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;

public class UploadExceptionReportToLogAnalyticsActivity
{
    private readonly ILoggingService _loggingService;

    public UploadExceptionReportToLogAnalyticsActivity(
        ILoggingService loggingService) =>
        _loggingService = loggingService;

    [FunctionName(nameof(UploadExceptionReportToLogAnalyticsActivity))]
    public Task RunAsync(
        [ActivityTrigger] ExceptionReport exceptionReport) =>
        _loggingService.LogExceptionAsync(LogDestinations.ErrorHandlingLog, exceptionReport);
}