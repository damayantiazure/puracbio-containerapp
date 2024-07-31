using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Pipeline;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using UpdateTagParameters = Microsoft.TeamFoundation.Build.WebApi.UpdateTagParameters;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class BuildRepository : IBuildRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public BuildRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<BuildDefinition?> GetBuildDefinitionByIdAsync(string organization, Guid projectId, int buildDefinitionId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildDefinitionRequest(organization, projectId, buildDefinitionId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<BuildDefinition?> GetBuildDefinitionByIdAsync(string organization, Guid projectId, int buildDefinitionId, PipelineProcessType pipelineProcessType, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildDefinitionRequest(organization, projectId, buildDefinitionId, pipelineProcessType, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BuildDefinition>?> GetBuildDefinitionsByProjectAsync(string organization, Guid projectId, bool includeAllProperties = false, CancellationToken cancellationToken = default)
    {
        var request = new GetAllBuildDefinitionsForProjectRequest(organization, projectId, includeAllProperties, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<BuildDefinition>?> GetBuildDefinitionsByProjectAsync(string organization, Guid projectId, PipelineProcessType pipelineProcessType, bool includeAllProperties = false, CancellationToken cancellationToken = default)
    {
        var request = new GetAllBuildDefinitionsForProjectRequest(organization, projectId, pipelineProcessType, includeAllProperties, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<string?> GetPipelineClassicBuildYaml(string organization, Guid projectId, int pipelineId, CancellationToken cancellationToken = default)
    {
        var request = new GetPipelineClassicBuildYamlRequest(organization, projectId, pipelineId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Yaml;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Change>?> GetBuildChangesAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildChangesRequest(organization, projectId, buildId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
    public async Task<Timeline?> GetBuildTimelineAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildTimelineRequest(organization, projectId, buildId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken));
    }

    /// <inheritdoc />
    public async Task<IEnumerable<Build>?> GetBuildAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildRequest(organization, projectId, buildId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<ProjectRetentionSetting?> GetProjectRetentionAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildRetentionRequest(organization, projectId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<ProjectRetentionSetting?> SetProjectRetentionAsync(string organization, Guid projectId, UpdateProjectRetentionSettingModel updateProjectRetentionSettingModel, CancellationToken cancellationToken = default)
    {
        var request = new SetBuildRetentionRequest(organization, projectId, updateProjectRetentionSettingModel, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>?> GetBuildTagsAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default)
    {
        var request = new GetBuildTagsRequest(organization, projectId, buildId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>?> AddTagsToBuildAsync(string organization, Guid projectId, int buildId, List<string> buildTags, CancellationToken cancellationToken = default)
    {
        var updateBuildTags = new UpdateTagParameters { tagsToAdd = buildTags };
        var request = new SetBuildTagsRequest(organization, projectId, buildId, updateBuildTags, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<string>?> RemoveTagsFromBuildAsync(string organization, Guid projectId, int buildId, List<string> buildTags, CancellationToken cancellationToken = default)
    {
        var updateBuildTags = new UpdateTagParameters { tagsToRemove = buildTags };
        var request = new SetBuildTagsRequest(organization, projectId, buildId, updateBuildTags, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }
}