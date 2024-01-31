#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Infra.StorageClient.Model;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class DeviationReportQueueTriggeredFunction
{
    private readonly ILogDeviationRecordProcess _logDeviationRecordProcess;

    public DeviationReportQueueTriggeredFunction(ILogDeviationRecordProcess logDeviationRecordProcess)
    {
        _logDeviationRecordProcess = logDeviationRecordProcess;
    }

    [FunctionName(nameof(DeviationReportQueueTriggeredFunction))]
    public System.Threading.Tasks.Task RunAsync(
        [QueueTrigger(StorageQueueNames.DeviationReportQueueName,
        Connection = "eventQueueStorageConnectionString")]
        string data) =>
            _logDeviationRecordProcess.LogDeviationReportRecord(data);
}