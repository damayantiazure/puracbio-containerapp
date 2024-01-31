using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask;

/// <summary>
/// Used to append a log to a task's log "{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/logs/{logId}".
/// </summary>
public class AppendToTaskLogRequest : HttpPostRequest<IDevHttpClientCallHandler, TaskLog, byte[]>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _hubName;
    private readonly string _planId;
    private readonly string _logId;

    protected override string Url => $"{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/logs/{_logId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public AppendToTaskLogRequest(string organization, Guid projectId, string hubName, Guid planId, int logId, byte[] stream, IDevHttpClientCallHandler httpClientCallHandler)
        : base(stream, httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _hubName = hubName;
        _planId = planId.ToString();
        _logId = logId.ToString();
    }
}