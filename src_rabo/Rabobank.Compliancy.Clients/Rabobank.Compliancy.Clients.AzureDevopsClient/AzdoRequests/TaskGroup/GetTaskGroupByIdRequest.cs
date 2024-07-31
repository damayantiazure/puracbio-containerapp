using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TaskGroup;

/// /// <summary>
/// Used to get the task group by id "{_organization}/{_project}/_apis/distributedtask/taskgroups/{_taskGroup}".
/// </summary>
public class GetTaskGroupByIdRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskGroup>>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _taskGroup;

    protected override string Url => $"{_organization}/{_project}/_apis/distributedtask/taskgroups/{_taskGroup}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.1-preview"}
    };

    public GetTaskGroupByIdRequest(string organization, Guid projectId, Guid taskGroupId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _taskGroup = taskGroupId.ToString();
    }
}