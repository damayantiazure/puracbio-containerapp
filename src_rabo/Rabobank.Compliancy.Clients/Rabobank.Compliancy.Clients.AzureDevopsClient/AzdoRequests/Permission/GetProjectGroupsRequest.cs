using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

/// <summary>
/// Used to get Projects by Id or Name from the URL "{_organization}_apis/projects/{_projectId}".
/// Allows for the parameter IncludeCapabilities passed through via the query string.
/// </summary>
public class GetProjectGroupsRequest : HttpGetRequest<IDevHttpClientCallHandler, ProjectGroup>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_identity/ReadScopedApplicationGroupsJson";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" },
    };

    public GetProjectGroupsRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}