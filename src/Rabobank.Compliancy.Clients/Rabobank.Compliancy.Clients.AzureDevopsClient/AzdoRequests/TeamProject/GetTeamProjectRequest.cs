using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.TeamFoundation.Core.WebApi;

/// <summary>
/// Used to get Projects by Id or Name from the URL "{_organization}_apis/projects/{_projectId}".
/// Allows for the parameter IncludeCapabilities passed through via the query string.
/// </summary>
public class GetTeamProjectRequest : HttpGetRequest<IDevHttpClientCallHandler, TeamProject>
{
    private readonly string _organization;
    private readonly string _projectIdOrName;
    private readonly bool _includeCapabilities;

    protected override string Url => $"{_organization}/_apis/projects/{_projectIdOrName}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "6.0"},
        {"includeCapabilities", _includeCapabilities.ToString()}
    };

    public GetTeamProjectRequest(string organization, Guid projectId, bool includeCapabilities, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectIdOrName = projectId.ToString();
        _includeCapabilities = includeCapabilities;
    }

    public GetTeamProjectRequest(string organization, string projectName, bool includeCapabilities, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectIdOrName = projectName;
        _includeCapabilities = includeCapabilities;
    }
}