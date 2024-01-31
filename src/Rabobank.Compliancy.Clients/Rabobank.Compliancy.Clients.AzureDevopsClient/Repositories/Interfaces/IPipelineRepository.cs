using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding Pipeline
/// </summary>
public interface IPipelineRepository
{
    /// <summary>
    /// Asks Azure Devops to generate YAML that resembles when the pipeline WOULD be run with the current configuration. Does not create any runs in Azure Devops.
    /// </summary>
    /// <param name="organization">The organization the pipeline belongs to</param>
    /// <param name="projectId">The project the pipeline belongs to</param>
    /// <param name="pipelineId">The pipeline of which the YAML will be generated</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable string representing the yaml of the pipeline.</returns>
    Task<string?> GetPipelineYamlFromPreviewRunAsync(string organization, Guid projectId, int pipelineId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a run by ID and PipelineID.
    /// </summary>
    /// <param name="organization">The organization the pipeline belongs to</param>
    /// <param name="projectId">The project the pipeline belongs to</param>
    /// <param name="pipelineId">The pipeline run belongs to</param>
    /// <param name="runId">The run itself.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="Run"/> that corresponds with the ID and belongs to the referened pipeline.</returns>
    /// <returns></returns>
    Task<Run?> GetPipelineRunAsync(string organization, Guid projectId, int pipelineId, int runId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns all approvers for a pipeline. 
    /// 
    /// Uses the approvalIds from a timeline to fetch all approvals corresponding to that
    /// timeline, then filters out approved-status steps and returns its approvers unique names
    /// </summary>
    /// <param name="organization">The organization the pipeline belongs to</param>
    /// <param name="projectId">The project the pipeline belongs to</param>
    /// <param name="timeline">The timeline of the build the approvals are for</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>A list of unique names of approvers</returns>
    Task<IEnumerable<string>?> GetBuildApproverByTimelineAsync(string organization, Guid projectId, Timeline? timeline, CancellationToken cancellationToken = default);
}