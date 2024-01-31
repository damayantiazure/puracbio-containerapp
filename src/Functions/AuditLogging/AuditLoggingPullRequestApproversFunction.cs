#nullable enable

using Microsoft.Azure.WebJobs;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class AuditLoggingPullRequestApproversFunction
{
    private const string _completedState = "completed";
    private readonly ILoggingService _loggingService;
    private readonly IPullRequestMergedEventParser _eventParser;

    public AuditLoggingPullRequestApproversFunction(
        ILoggingService loggingService,
        IPullRequestMergedEventParser eventParser)
    {
        _loggingService = loggingService;
        _eventParser = eventParser;
    }

    [FunctionName(nameof(AuditLoggingPullRequestApproversFunction))]
    public async Task RunAsync(
        [QueueTrigger(StorageQueueNames.AuditPullRequestApproversQueueName,
            Connection = "eventQueueStorageConnectionString")]
        string data)
    {
        var evt = default(PullRequestMergedEvent);

        try
        {
            evt = _eventParser.Parse(data);

            if (!string.Equals(evt.Status, _completedState, StringComparison.InvariantCultureIgnoreCase))
            {
                return;
            }

            var report = CreateReport(evt);

            await _loggingService.LogInformationAsync(LogDestinations.AuditPullRequestApproversLog, report);
        }
        catch (Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (nameof(AuditLoggingPullRequestApproversFunction), evt?.Organization, evt?.ProjectId)
            {
                PullRequestUrl = evt?.PullRequestUrl,
                RequestData = data
            };
            await _loggingService.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog, exceptionBaseMetaInformation, e);
            throw;
        }
    }

    private static AuditLoggingPullRequestReport CreateReport(PullRequestMergedEvent mergeEvent)
    {
        var report = new AuditLoggingPullRequestReport
        {
            Approvers = mergeEvent.Approvers,
            ClosedDate = mergeEvent.ClosedDate,
            CreationDate = mergeEvent.CreationDate,
            LastMergeCommitId = mergeEvent.LastMergeCommitId,
            LastMergeSourceCommit = mergeEvent.LastMergeSourceCommit,
            LastMergeTargetCommit = mergeEvent.LastMergeTargetCommit,
            Organization = mergeEvent.Organization,
            ProjectId = mergeEvent.ProjectId,
            ProjectName = mergeEvent.ProjectName,
            PullRequestId = mergeEvent.PullRequestId,
            PullRequestUrl = mergeEvent.PullRequestUrl,
            RepositoryId = mergeEvent.RepositoryId,
            RepositoryUrl = mergeEvent.RepositoryUrl,
            Status = mergeEvent.Status,
            CreatedBy = mergeEvent.CreatedBy,
            ClosedBy = mergeEvent.ClosedBy
        };

        return report;
    }
}