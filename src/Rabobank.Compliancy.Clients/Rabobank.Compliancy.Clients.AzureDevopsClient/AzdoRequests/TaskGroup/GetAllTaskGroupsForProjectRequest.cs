using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TaskGroup;

/// <summary>
/// Used to get all TaskGroups for a project from the URL "{_organization}/{_projectId}/_apis/distributedtask/taskgroups".
/// </summary>
public class GetAllTaskGroupsForProjectRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.DistributedTask.WebApi.TaskGroup>>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/distributedtask/taskgroups";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetAllTaskGroupsForProjectRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}