using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class ReleaseRepository : IReleaseRepository
{
    private readonly IVsrmHttpClientCallHandler _httpClientCallHandler;

    public ReleaseRepository(IVsrmHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<ReleaseDefinition?> GetReleaseDefinitionByIdAsync(string organization, Guid projectId, int releaseDefinitionId, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseDefinitionRequest(organization, projectId, releaseDefinitionId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<string?> GetReleaseDefinitionRevisionByIdAsync(string organization, Guid projectId, int releaseDefinitionId, int revisionId, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseDefinitionRevisionRequest(organization, projectId, releaseDefinitionId, revisionId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReleaseDefinition>?> GetReleaseDefinitionsByProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetAllReleaseDefinitionsForProjectRequest(organization, projectId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<ReleaseApproval>?> GetReleaseApprovalsByReleaseIdAsync(string organization, Guid projectId, int releaseId, ApprovalStatus status, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseApprovalsRequest(organization, projectId, releaseId, status, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<ReleaseSettings?> GetReleaseSettingsAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseSettingsRequest(organization, projectId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>?> GetReleaseTagsAsync(string organization, Guid projectId, int releaseId, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseTagsRequest(organization, projectId, releaseId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<string?> GetReleaseTaskLogByTaskIdAsync(string organization, Guid projectId, int releaseId, int environmentId, int releaseDeployPhaseId, int taskId, CancellationToken cancellationToken = default)
    {
        var request = new GetReleaseTaskLogRequest(organization, projectId, releaseId, environmentId, releaseDeployPhaseId, taskId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}