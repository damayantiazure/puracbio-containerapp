using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;

using Microsoft.TeamFoundation.Core.WebApi;

/// <summary>
/// Used to get Projects by Id from the URL "{_organization}/_apis/Contribution/HierarchyQuery/project/{_projectId}".
/// Gets the project info through URL 
/// </summary>
public class GetProjectInfoRequest : HttpGetRequest<IDevHttpClientCallHandler, ProjectInfo>
{
    private readonly string _organization;
    private readonly string _projectId;

    protected override string Url => $"{_organization}/_apis/Contribution/HierarchyQuery/project/{_projectId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "6.0"}
    };

    public GetProjectInfoRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId.ToString();
    }
}