using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;

internal class ProjectInitRepository : IProjectInitRepository
{
    private readonly IProjectInitHttpClientCallHandler _httpClientCallHandler;

    public ProjectInitRepository(IProjectInitHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    public async Task CreateNewProject(string organization, string projectName, string userEmailAddress, string projectInitCode, CancellationToken cancellationToken = default)
    {
        await new CreateProjectInitRequest(organization, projectName, userEmailAddress, projectInitCode, _httpClientCallHandler).ExecuteAsync(cancellationToken: cancellationToken);
    }
}