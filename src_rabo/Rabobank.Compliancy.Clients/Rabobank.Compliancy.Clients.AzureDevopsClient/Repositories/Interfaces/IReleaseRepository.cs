using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding <see cref="Release"/>
/// </summary>
public interface IReleaseRepository
{
    /// <summary>
    /// Gets a release definition by ID.
    /// </summary>
    /// <param name="organization">The organization the ReleaseDefinition belongs to</param>
    /// <param name="projectId">The project the ReleaseDefinition belongs to</param>
    /// <param name="releaseDefinitionId">The ID of the ReleaseDefinition</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="ReleaseDefinition"/> representing a ReleaseDefinition (Classic Pipeline) the way Azure Devops API returns it.</returns>
    Task<ReleaseDefinition?> GetReleaseDefinitionByIdAsync(string organization, Guid projectId, int releaseDefinitionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a release definition, provides the possibility to filter by revision.
    /// </summary>
    /// <param name="organization">The organization the ReleaseDefinition belongs to</param>
    /// <param name="projectId">The project the ReleaseDefinition belongs to</param>
    /// <param name="releaseDefinitionId">The ID of the ReleaseDefinition</param>
    /// <param name="revisionId">The ID of the Revision</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable string representing a revision (of a Classic Pipeline) the way Azure Devops API returns it.</returns>
    Task<string?> GetReleaseDefinitionRevisionByIdAsync(string organization, Guid projectId, int releaseDefinitionId, int revisionId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get's all release definitions for a project.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the definitions belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="ReleaseDefinition"/> representing a BuildDefinition (pipeline) the way Azure Devops API returns it.</returns>
    Task<IEnumerable<ReleaseDefinition>?> GetReleaseDefinitionsByProjectAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the approvals for a release by approval status.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the definitions belong to</param>
    /// <param name="releaseId">Id of the release.</param>
    /// <param name="status">Approval status
    /// - approved string Indicates the approval is approved.
    /// - canceled string Indicates the approval is canceled.
    /// - pending string Indicates the approval is pending.
    /// - reassigned string Indicates the approval is reassigned.
    /// - rejected string Indicates the approval is rejected.
    /// - skipped string Indicates the approval is skipped.
    /// - undefined string Indicates the approval does not have the status set.
    /// </param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="ReleaseApproval"/> representing a list of release approvals.</returns>
    Task<IEnumerable<ReleaseApproval>?> GetReleaseApprovalsByReleaseIdAsync(string organization, Guid projectId, int releaseId, ApprovalStatus status, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get release settings.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the definitions belong to</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable <see cref="ReleaseSettings"/> representing the release settings.</returns>
    Task<ReleaseSettings?> GetReleaseSettingsAsync(string organization, Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the tags for a specific release.
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the definitions belong to</param>
    /// <param name="releaseId">Id of the release.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="string"/> representing a list of tags belonging to the release.</returns>
    Task<IEnumerable<string>?> GetReleaseTagsAsync(string organization, Guid projectId, int releaseId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get's the task-log for a specific release
    /// </summary>
    /// <param name="organization">The organization the Project belongs to</param>
    /// <param name="projectId">The project the release belong to</param>
    /// <param name="releaseId">The release the logs belong to</param>
    /// <param name="environmentId">The environment the logs are about</param>
    /// <param name="releaseDeployPhaseId">The phase the logs are about</param>
    /// <param name="taskId">The task the logs are about (task-logs!)</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns>Nullable IEnumerable of <see cref="string"/> representing the log data.</returns>
    Task<string?> GetReleaseTaskLogByTaskIdAsync(string organization, Guid projectId, int releaseId, int environmentId, int releaseDeployPhaseId, int taskId, CancellationToken cancellationToken = default);
}