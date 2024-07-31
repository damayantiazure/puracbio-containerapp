using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.DistributedTask;

/// <summary>
/// Used to add a task started event "{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/events".
/// </summary>
public class AddTaskStartedEventRequest : HttpPostRequest<IDevHttpClientCallHandler, string, AddTaskEventBodyContent>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _hubName;
    private readonly string _planId;

    protected override string Url => $"{_organization}/{_project}/_apis/distributedtask/hubs/{_hubName}/plans/{_planId}/events";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public AddTaskStartedEventRequest(string organization, Guid projectId, string hubName, Guid planId, AddTaskEventBodyContent taskEvent, IDevHttpClientCallHandler httpClientCallHandler)
        : base(taskEvent, httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _hubName = hubName;
        _planId = planId.ToString();
    }
}