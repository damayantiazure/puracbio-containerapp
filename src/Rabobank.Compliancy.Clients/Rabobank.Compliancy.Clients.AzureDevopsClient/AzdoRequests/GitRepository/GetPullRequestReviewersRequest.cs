using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;

/// <summary>
/// Used to get all reviewers of a pullrequest by pullrequest ID from the URL "{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullRequests/{_pullRequestId}/reviewers".
/// </summary>
public class GetPullRequestReviewersRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Microsoft.TeamFoundation.SourceControl.WebApi.IdentityRefWithVote>>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _repositoryId;
    private readonly string _pullRequestId;

    protected override string Url => $"{_organization}/{_project}/_apis/git/repositories/{_repositoryId}/pullRequests/{_pullRequestId}/reviewers";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetPullRequestReviewersRequest(string organization, Guid projectId, Guid repositoryId, int pullRequestId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _repositoryId = repositoryId.ToString();
        _pullRequestId = pullRequestId.ToString();
    }
}