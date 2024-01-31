namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;

internal interface IProjectInitRepository
{
    Task CreateNewProject(string organization, string projectName, string userEmailAddress, string projectInitCode, CancellationToken cancellationToken = default);
}