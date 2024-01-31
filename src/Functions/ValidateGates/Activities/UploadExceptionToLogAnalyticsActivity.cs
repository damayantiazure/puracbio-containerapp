#nullable enable

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ValidateGates.Activities;

public class UploadExceptionToLogAnalyticsActivity
{
    private readonly ILoggingService _loggingService;

    public UploadExceptionToLogAnalyticsActivity(ILoggingService loggingService)
        => _loggingService = loggingService;

    [FunctionName(nameof(UploadExceptionToLogAnalyticsActivity))]
    public async Task RunAsync(
        [ActivityTrigger] ExceptionReport report)
    {
        await _loggingService.LogExceptionAsync(LogDestinations.ValidateGatesErrorLog, report);
    }
}