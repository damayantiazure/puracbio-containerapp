using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

/// <summary>
/// Used to get all Environments for a project from the URL "{_organization}/{_projectId}/_apis/distributedtask/environments".
///
/// This request requires the following permissions
///  - ?
/// </summary>
public class GetEnvironmentInstancesRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.DistributedTask.WebApi.EnvironmentInstance>>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/distributedtask/environments";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetEnvironmentInstancesRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}