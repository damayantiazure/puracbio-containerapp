#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Threading.Tasks;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class AuditLoggingClassicReleasePoisonQueueFunction
{
    private readonly ILoggingService _loggingService;

    public AuditLoggingClassicReleasePoisonQueueFunction(ILoggingService loggingService) => 
        _loggingService = loggingService;

    [FunctionName(nameof(AuditLoggingClassicReleasePoisonQueueFunction))]
    public async Task RunAsync([QueueTrigger($"{StorageQueueNames.AuditClassicReleaseQueueName}-poison",
        Connection = "eventQueueStorageConnectionString")] string message)
    {
        var poisonMessageReport = new PoisonMessageReport
        {
            FailedQueueTrigger = StorageQueueNames.AuditClassicReleaseQueueName,
            MessageText = message
        };

        await _loggingService.LogInformationAsync(LogDestinations.AuditPoisonMessagesLog, poisonMessageReport);
    }
}