#nullable enable

using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.AuditLogging.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Services;

public class MonitorDecoratorService : IMonitorDecoratorService
{
    public static readonly IEnumerable<string> SuccessMessages = new[]
    {
        DecoratorResultMessages.Passed,
        DecoratorResultMessages.NotRegistered,
        DecoratorResultMessages.InvalidYaml,
        DecoratorResultMessages.AlreadyScanned,
        DecoratorResultMessages.WarningAlreadyScanned,
        DecoratorResultMessages.ExclusionList,
        DecoratorResultMessages.NotCompliant,
        DecoratorResultMessages.WarningNotCompliant,
        DecoratorResultMessages.NoProdStagesFound,
        DecoratorErrors.ErrorPrefix
    };

    private static readonly Result?[] _recordIsExecuted =
        { Result.failed, Result.succeeded, Result.succeededWithIssues };

    private static readonly Status?[] _taskIsExecuted =
    {
        Status.failed, Status.failure, Status.success, Status.succeeded,
        Status.partiallySucceeded
    };

    private readonly IAzdoRestClient _azdoRestClient;
    private readonly ILoggingService _loggingService;

    public MonitorDecoratorService(IAzdoRestClient azdoRestClient, ILoggingService loggingService)
    {
        _azdoRestClient = azdoRestClient;
        _loggingService = loggingService;
    }

    public async Task MonitorDecoratorYamlReleaseAsync(string organization, string projectId, string runId,
        string stageName)
    {
        var timeline = await _azdoRestClient.GetAsync(Builds.Timeline(projectId, runId), organization);
        if (timeline == null)
        {
            return;
        }

        var decoratorRecords = GetDecoratorRecordsForStage(stageName, timeline.Records);
        if (!decoratorRecords.Any())
        {
            return;
        }

        var logs = await Task.WhenAll(decoratorRecords
            .Select(async r =>
                await _azdoRestClient.GetAsStringAsync(Builds.GetLogs(projectId, runId, r.Log.Id), organization)));

        await LogDecoratorFailuresAsync(organization, projectId, runId, null, ItemTypes.YamlReleasePipeline,
            stageName, logs);
    }

    public async Task MonitorDecoratorClassicReleaseAsync(string organization, string projectId, Release release,
        string stageName)
    {
        var stage = release.Environments?.FirstOrDefault(e => e.Name == stageName);
        if (stage == null)
        {
            return;
        }

        var releaseDeployPhases = stage.DeploySteps.SelectMany(d => d.ReleaseDeployPhases);

        foreach (var releaseDeployPhase in releaseDeployPhases)
        {
            var tasks = releaseDeployPhase.DeploymentJobs.SelectMany(d => d.Tasks);
            var decoratorTasks = tasks
                .Where(t => t.Name.StartsWith("Pre-job: check pipeline registration and compliancy") &&
                            t.Id != "0" &&
                            _taskIsExecuted.Contains(t.Status)).ToList();

            if (!decoratorTasks.Any())
            {
                return;
            }

            var logs = await Task.WhenAll(decoratorTasks
                .Select(async t => await _azdoRestClient.GetAsStringAsync(ReleaseManagement.TaskLogs(
                    projectId, release.Id, stage.Id, releaseDeployPhase.Id, t.Id), organization)));

            await LogDecoratorFailuresAsync(organization, projectId, null, release.Id.ToString(),
                ItemTypes.ClassicReleasePipeline,
                stageName, logs);
        }
    }

    private static IEnumerable<TimelineRecord> GetDecoratorRecordsForStage(string stageName,
        IEnumerable<TimelineRecord> records)
    {
        var stage = records.FirstOrDefault(r => r.Type == "Stage" && r.Identifier == stageName);

        if (stage == null)
        {
            return Enumerable.Empty<TimelineRecord>();
        }

        var phaseIds = records.Where(r => r.Type == "Phase" && r.ParentId == stage.Id)
            .Select(r => r.Id);

        var jobIds = records.Where(r => r.ParentId != null && r.Type == "Job" && phaseIds.Contains(r.ParentId.Value))
            .Select(r => r.Id);

        var decoratorRecords = records
            .Where(r => r.ParentId != null &&
                        jobIds.Contains(r.ParentId.Value) &&
                        r.Name.StartsWith("Pre-job: check pipeline registration and compliancy") &&
                        r.Log != null &&
                        _recordIsExecuted.Contains(r.Result));

        return decoratorRecords;
    }

    private async Task LogDecoratorFailuresAsync(string organization, string projectId, string? runId,
        string? releaseId, string pipelineType, string stageName, IEnumerable<string> logs)
    {
        var messagesToExclude = SuccessMessages.Select(m => m.RemoveNewlines());
        var failures = logs.Where(log => !messagesToExclude.Any(log
            .RemoveUniversalDateTimeString()
            .RemoveNewlines()
            .Contains));

        foreach (var failure in failures)
        {
            var report = new DecoratorErrorReport
            {
                Organization = organization,
                ProjectId = projectId,
                RunId = runId,
                ReleaseId = releaseId,
                PipelineType = pipelineType,
                StageName = stageName,
                Message = failure
            };

            await _loggingService.LogInformationAsync(LogDestinations.DecoratorErrorLog, report);
        }
    }
}