using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public class RepositoryService : IRepositoryService
{
    private readonly IAzdoRestClient _azdoClient;

    public RepositoryService(IAzdoRestClient azdoClient) => _azdoClient = azdoClient;

    public async Task<Uri> GetUrlAsync(string organization,
        Response.Project pipelineProject, Response.Repository repository)
    {
        var projectId = string.IsNullOrEmpty(repository.Project?.Id)
            ? await GetProjectIdByNameAsync(organization, pipelineProject, repository)
            : repository.Project.Id;

        return new Uri($"https://dev.azure.com/{organization}/{projectId}/_git/{repository.Id}");
    }

    public async Task<string> GetProjectIdByNameAsync(string organization,
        Response.Project pipelineProject, Response.Repository repository)
    {
        //The repo url contains the names of the project & repo, but the ids have to be logged.
        var urlWithNames = repository.Url.AbsoluteUri;
        var projectName = Regex.Match(urlWithNames, $"dev.azure.com/{organization}/(.+?)/_git/")
            .Groups[1].Value;
        return projectName == pipelineProject.Name
            ? pipelineProject.Id
            : (await _azdoClient.GetAsync(Project.ProjectByName(projectName), organization)).Id;
    }
}