using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;

/// <summary>
/// Used to get all pull requests for a repository by repository ID from the URL "{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullrequests".
/// </summary>
public class GetAllPullRequestsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequest>>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _repositoryId;

    protected override string Url => $"{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullrequests";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetAllPullRequestsRequest(string organization, Guid projectId, Guid repositoryId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _repositoryId = repositoryId.ToString();
    }
}