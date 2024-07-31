using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.WorkItemTracking;

/// <summary>
/// Used to get the results of the query given its WIQL "{_organization}/{_project}/{_team}/_apis/wit/wiql".
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/wit/wiql/query-by-wiql?view=azure-devops-rest-7.1&amp;tabs=HTTP#request-body
/// </summary>
public class GetQueryByWiqlRequest : HttpPostRequest<IDevHttpClientCallHandler, WorkItemQueryResult, GetQueryBodyContent>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _team;

    protected override string Url => $"{_organization}/{_project}/{_team}/_apis/wit/wiql";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview" }
    };

    public GetQueryByWiqlRequest(string organization, Guid projectId, string team, GetQueryBodyContent query, IDevHttpClientCallHandler httpClientCallHandler)
        : base(query, httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _team = team;
    }
}