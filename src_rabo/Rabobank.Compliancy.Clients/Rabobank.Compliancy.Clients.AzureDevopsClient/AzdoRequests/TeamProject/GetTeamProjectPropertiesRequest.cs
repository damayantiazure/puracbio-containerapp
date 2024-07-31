using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.TeamFoundation.Core.WebApi;

/// <summary>
/// Used to get Projects properties from the URL "{_organization}/_apis/projects/{_projectId}/properties".
/// </summary>
public class GetTeamProjectPropertiesRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<ProjectProperty>>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/_apis/projects/{_projectId}/properties";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.1-preview"}
    };

    public GetTeamProjectPropertiesRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}