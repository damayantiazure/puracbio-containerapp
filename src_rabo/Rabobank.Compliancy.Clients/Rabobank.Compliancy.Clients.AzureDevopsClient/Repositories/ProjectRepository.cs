using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.TeamProject;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class ProjectRepository : IProjectRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public ProjectRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<Operation?> CreateProjectAsync(string organization, string projectName, string description, CancellationToken cancellationToken = default)
    {
        var request = new CreateProjectRequest(organization, projectName, description, _httpClientCallHandler);
        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Operation?> DeleteProjectAsync(string organization, Guid id, CancellationToken cancellationToken = default)
    {
        var request = new DeleteProjectRequest(organization, id, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TeamProject?> GetProjectByIdAsync(string organization, Guid id, bool includeCapabilities, CancellationToken cancellationToken = default)
    {
        var request = new GetTeamProjectRequest(organization, id, includeCapabilities, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<TeamProject?> GetProjectByNameAsync(string organization, string projectName, bool includeCapabilities, CancellationToken cancellationToken = default)
    {
        var request = new GetTeamProjectRequest(organization, projectName, includeCapabilities, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<TeamProject>?> GetProjectsAsync(string organization, CancellationToken cancellationToken = default)
    {
        var request = new GetTeamProjectsRequest(organization, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ProjectProperty>?> GetProjectPropertiesAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetTeamProjectPropertiesRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<ProjectInfo?> GetProjectInfoAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetProjectInfoRequest(organization, projectId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}