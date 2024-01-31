using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.TeamFoundation.Core.WebApi;

/// <summary>
/// Used to get Projects by Id or Name from the URL "{_organization}_apis/projects/{_projectId}".
/// Allows for the parameter IncludeCapabilities passed through via the query string.
/// </summary>
public class GetTeamProjectsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<TeamProject>>
{
    private readonly string _organization;

    protected override string Url => $"{_organization}/_apis/projects/";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetTeamProjectsRequest(string organization, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
    }
}