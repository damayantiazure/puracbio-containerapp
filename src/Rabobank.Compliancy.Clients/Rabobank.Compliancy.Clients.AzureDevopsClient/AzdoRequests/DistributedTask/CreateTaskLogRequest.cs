using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask;

/// <summary>
/// Used to create a log and connect it to a pipeline run's execution plan "{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/logs".
/// </summary>
public class CreateTaskLogRequest : HttpPostRequest<IDevHttpClientCallHandler, TaskLog, TaskLog>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _hubName;
    private readonly string _planId;

    protected override string Url => $"{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/logs";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public CreateTaskLogRequest(string organization, Guid projectId, string hubName, Guid planId, TaskLog taskLog, IDevHttpClientCallHandler httpClientCallHandler)
        : base(taskLog, httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _hubName = hubName;
        _planId = planId.ToString();
    }
}