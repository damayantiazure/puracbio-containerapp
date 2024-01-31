using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;

/// <summary>
/// Used to get a pull requests by pullrequest ID from the URL "{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullrequests/{_pullRequestId}".
/// </summary>
public class GetPullRequestRequest : HttpGetRequest<IDevHttpClientCallHandler, Microsoft.TeamFoundation.SourceControl.WebApi.GitPullRequest>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _repositoryId;
    private readonly string _pullRequestId;

    protected override string Url => $"{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullrequests/{_pullRequestId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetPullRequestRequest(string organization, Guid projectId, Guid repositoryId, int pullRequestId, IDevHttpClientCallHandler httpClientCallhandler)
        : base(httpClientCallhandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _repositoryId = repositoryId.ToString();
        _pullRequestId = pullRequestId.ToString();
    }
}