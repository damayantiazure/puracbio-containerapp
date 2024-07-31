using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.GitRepository;

/// <summary>
/// Used to get a Git Repository by ID from the URL "{_organization}/{_project}/_apis/repositories/{_gitRepoId}".
/// </summary>
public class GetGitRepositoryRequest : HttpGetRequest<IDevHttpClientCallHandler, Microsoft.TeamFoundation.SourceControl.WebApi.GitRepository>
{
    private readonly string _organization;
    private readonly string _project;
    private readonly string _gitRepo;

    protected override string Url => $"{_organization}/{_project}/_apis/git/repositories/{_gitRepo}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetGitRepositoryRequest(string organization, Guid projectId, Guid gitRepoId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectId.ToString();
        _gitRepo = gitRepoId.ToString();
    }

    public GetGitRepositoryRequest(string organization, string projectName, string gitRepoName, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _project = projectName;
        _gitRepo = gitRepoName;
    }
}