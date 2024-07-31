using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;

/// <summary>
/// Used to get all Git Repositories by project from the URL "{_organization}/{_project}/_apis/repositories/".
/// </summary>
public class GetAllGitRepositoriesForProjectRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository>>
{
    private readonly string _organization;
    private readonly string _project;

    protected override string Url => $"{_organization}/{_project}/_apis/git/repositories/";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetAllGitRepositoriesForProjectRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
    }
}