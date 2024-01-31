#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Threading.Tasks;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class AuditLoggingYamlReleasePoisonQueueFunction
{
    private readonly ILoggingService _loggingService;

    public AuditLoggingYamlReleasePoisonQueueFunction(ILoggingService loggingService) => 
        _loggingService = loggingService;

    [FunctionName(nameof(AuditLoggingYamlReleasePoisonQueueFunction))]
    public async Task RunAsync([QueueTrigger($"{StorageQueueNames.AuditYamlReleaseQueueName}-poison",
        Connection = "eventQueueStorageConnectionString")] string message)
    {
        var poisonMessageReport = new PoisonMessageReport
        {
            FailedQueueTrigger = StorageQueueNames.AuditYamlReleaseQueueName,
            MessageText = message
        };

        await _loggingService.LogInformationAsync(LogDestinations.AuditPoisonMessagesLog, poisonMessageReport);
    }
}