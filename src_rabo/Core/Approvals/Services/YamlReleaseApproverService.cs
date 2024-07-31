using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rabobank.Compliancy.Core.Approvals.Utils;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Project = Rabobank.Compliancy.Infra.AzdoClient.Response.Project;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Approvals.Services;

public class YamlReleaseApproverService : IYamlReleaseApproverService
{
    private const int MaxParallelThreads = 10;
    private readonly IAzdoRestClient _client;

    public YamlReleaseApproverService(IAzdoRestClient client) => _client = client;

    public async Task<bool> HasApprovalAsync(Project project, string runId, string exclude, string organization = null)
    {
        var stageIds = await GetStageIdsAsync(project.Id, runId, organization);
        foreach (var stageId in stageIds)
        {
            var approvers = await GetStageApproversAsync(project.Id, runId, stageId, project.Name, organization);
            if (approvers.Any(a => IsValidApprover(a, exclude)))
            {
                return true;
            }
        }

        return false;
    }

    public async Task<IEnumerable<string>> GetAllApproversAsync(Project project, string runId, string organization = null)
    {
        var stageIds = await GetStageIdsAsync(project.Id, runId, organization);
        var mutex = new SemaphoreSlim(MaxParallelThreads);

        return (await Task.WhenAll(
                stageIds.Select(async (s, i) =>
                {
                    await mutex.WaitAsync();
                    try
                    {
                        return await GetStageApproversAsync(project.Id, runId, s, project.Name, organization);
                    }
                    finally { mutex.Release(); }
                })))
            .SelectMany(a => a)
            .Distinct()
            .Where(IsValidApprover);
    }

    private async Task<IEnumerable<string>> GetStageIdsAsync(string projectId, string runId, string organization)
    {
        var timeline = await _client.GetAsync(Builds.Timeline(projectId, runId), organization);
        var records = timeline?.Records;
        return records?.Where(r => r.Type == "Stage")
            .OrderBy(r => r.StartTime.HasValue)
            .ThenByDescending(r => r.StartTime)
            .Select(r => r.Id.ToString("B", CultureInfo.InvariantCulture));
    }

    private async Task<IEnumerable<string>> GetStageApproversAsync(
        string projectId,
        string runId,
        string stageId,
        string projectName,
        string organization)
    {
        var jObject = await _client.PostAsync(HierarchyQuery.ProjectInfo(projectId),
            HierarchyQuery.Approvals(projectName, runId, stageId), organization, true);

        var serializer = JsonSerializer.CreateDefault(new JsonSerializerSettings
            { DateTimeZoneHandling = DateTimeZoneHandling.Utc });

        var steps = jObject.SelectToken(
            "dataProviders.['ms.vss-build-web.checks-panel-data-provider'][0].approvals[-1:].steps");

        return steps == null
            ? Enumerable.Empty<string>()
            : steps?
                .OrderByDescending(s => s.SelectToken("lastModifiedOn")?.ToObject<DateTime>(serializer))
                .Select(o => o.SelectToken("actualApprover.uniqueName")?.ToString());
    }

    private static bool IsValidApprover(string approver, string exclude) =>
        approver != null && MailChecker.IsValidEmail(approver)
                         && approver != exclude;

    private static bool IsValidApprover(string approver) =>
        IsValidApprover(approver, null);
}