using Microsoft.Extensions.DependencyInjection;
using Microsoft.TeamFoundation.Core.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.Helpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;

public class ProjectInitFixture : IAsyncLifetime
{
    public string Organization { get; private set; }
    public string ProjectName { get; private set; }
    public IServiceProvider ServiceProvider { get; private set; }

    private readonly IProjectRepository _projectRepository;
    private readonly IProjectInitRepository _projectInitRepository;
    private readonly IOperationRepository _operationRepository;
    private TeamProject? _project;

    public ProjectInitFixture()
    {
        ConfigurationHelper.ConfigureDefaultFiles();
        var serviceCollection = new ServiceCollection().AddProjectInitDependencies();
        ServiceProvider = ServiceProviderHelper.InitAzureDevopsClient(serviceCollection);

        Organization = Environment.GetEnvironmentVariable("organization") ?? throw new InvalidOperationException("AppSettings is missing the organization setting");
        ProjectName = $"IntegrationTests-{Guid.NewGuid()}";

        _projectRepository = ServiceProvider.GetServiceOrThrow<IProjectRepository>();
        _projectInitRepository = ServiceProvider.GetServiceOrThrow<IProjectInitRepository>();
        _operationRepository = ServiceProvider.GetServiceOrThrow<IOperationRepository>();
    }

    public async Task<TeamProject> GetProjectAsync()
    {
        _project ??= await _projectRepository.GetProjectByNameAsync(Organization, ProjectName, false) ?? throw new InvalidOperationException($"Project should have been created but was not found in Azure Devops by name {ProjectName}");
        return _project;
    }

    public async Task InitializeAsync()
    {
        var userEmailAddress = Environment.GetEnvironmentVariable("userEmailAddress") ?? throw new InvalidOperationException("AppSettings is missing the userEmailAddress setting");
        var projectInitCode = Environment.GetEnvironmentVariable("projectInitCode") ?? throw new InvalidOperationException("AppSettings is missing the projectInitCode setting");
        await _projectInitRepository.CreateNewProject(Organization, ProjectName, userEmailAddress, projectInitCode);
    }

    public async Task DisposeAsync()
    {
        var project = await GetProjectAsync();
        if (project.Id == Guid.Empty)
        {
            return;
        }

        var operation = await _projectRepository.DeleteProjectAsync(Organization, project.Id);

        await _operationRepository.EnsureOperationCompletedAsync(Organization, operation?.Id);
    }
}