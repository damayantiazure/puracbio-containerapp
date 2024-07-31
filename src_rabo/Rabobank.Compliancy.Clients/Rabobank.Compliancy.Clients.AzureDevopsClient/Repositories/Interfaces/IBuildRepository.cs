using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="BuildDefinition"/>
/// </summary>
public interface IBuildRepository
{
    /// <summary>
    /// Gets a builddefinition by ID.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the BuildDefinition belongs to</param>
    /// <param name="buildDefinitionId">The ID of the BuildDefinition</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="BuildDefinition"/> representing a BuildDefinition (pipeline) the way Azure Devops API returns it.</returns>
    Task<BuildDefinition?> GetBuildDefinitionByIdAsync(string organization, Guid projectId, int buildDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all builddefinitions belonging to a certain project
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="includeAllProperties">By default, the List function of this endpoint does not return all properties, this will make sure it does</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="BuildDefinition"/> representing a BuildDefinition (pipeline) the way Azure Devops API returns it.</returns>
    Task<IEnumerable<BuildDefinition>?> GetBuildDefinitionsByProjectAsync(string organization, Guid projectId, bool includeAllProperties = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all builddefinitions belonging to a certain project
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="pipelineProcessType">By default, the List function of this endpoint returns all pipelines. This filter allows you to differentiate between DesignerBuild and Yaml. Passing a type other than those two will not change the resultset</param>
    /// <param name="includeAllProperties">By default, the List function of this endpoint does not return all properties, this will make sure it does</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="BuildDefinition"/> representing a BuildDefinition (pipeline) the way Azure Devops API returns it.</returns>
    Task<IEnumerable<BuildDefinition>?> GetBuildDefinitionsByProjectAsync(string organization, Guid projectId, PipelineProcessType pipelineProcessType, bool includeAllProperties = false, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the generated yaml for a classic build pipeline
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="pipelineId">The builddefinition we want the yaml for</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<string?> GetPipelineClassicBuildYaml(string organization, Guid projectId, int pipelineId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the changes associated with a build
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of <see cref="Change"/> that represents a change associated with a build.</returns>
    Task<IEnumerable<Change>?> GetBuildChangesAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the timeline for a specific build.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of <see cref="Timeline"/> that represents the timeline of a build.</returns>
    Task<Timeline?> GetBuildTimelineAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the details of the build.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of <see cref="Build"/> that represents the build information.</returns>
    Task<IEnumerable<Build>?> GetBuildAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the build retention settings.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A project retention setting of the <see cref="ProjectRetentionSetting"/> class.</returns>
    Task<ProjectRetentionSetting?> GetProjectRetentionAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the build retention settings.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="updateProjectRetentionSettingModel">The retention settings to be updated.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A project retention setting of the <see cref="ProjectRetentionSetting"/> class.</returns>
    Task<ProjectRetentionSetting?> SetProjectRetentionAsync(string organization, Guid projectId, UpdateProjectRetentionSettingModel updateProjectRetentionSettingModel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the build tags.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of <see cref="string"/> that represents the build tags.</returns>
    Task<IEnumerable<string>?> GetBuildTagsAsync(string organization, Guid projectId, int buildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add a new tags to a build.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="buildTags">A collection of build tags to be added.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of <see cref="string"/> that represents the build tags.</returns>
    Task<IEnumerable<string>?> AddTagsToBuildAsync(string organization, Guid projectId, int buildId, List<string> buildTags, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes a tags from the build.
    /// </summary>
    /// <param name="organization">The organization the BuildDefinition belongs to</param>
    /// <param name="projectId">The project the builddefinitions belong to</param>
    /// <param name="buildId">The identifier of the build</param>
    /// <param name="buildTags">A collection of build tags to be removed.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>A collection of nullable strings that represents the build tags.</returns>
    Task<IEnumerable<string>?> RemoveTagsFromBuildAsync(string organization, Guid projectId, int buildId, List<string> buildTags, CancellationToken cancellationToken = default);
}