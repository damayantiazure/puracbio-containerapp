using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// Service definitions for the <see cref="Pipeline"/>s.
/// </summary>
public interface IPipelineService
{
    /// <summary>
    /// Gets a pipeline without doing any additional calls for information. This method is intended to replace the GetPipelineAsync which uses PipelineProcessType
    /// to determine the manner in which a pipeline is retrieved
    /// </summary>
    /// <typeparam name="TPipeline"></typeparam>
    /// <param name="project"></param>
    /// <param name="pipelineId"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<Pipeline> GetPipelineAsync<TPipeline>(Project project, int pipelineId,
        CancellationToken cancellationToken = default) where TPipeline : Pipeline;

    /// <summary>
    /// Gets a pipeline without doing any additional calls for information.
    /// </summary>
    /// <param name="project">An instance of the project as <see cref="Project"/>.</param>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="definitionType">The pipeline process type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Pipeline"/>.</returns>
    Task<Pipeline> GetPipelineAsync(Project project, int pipelineId, PipelineProcessType definitionType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a pipeline including all information nessecary for scanning.
    /// </summary>
    /// <param name="project">An instance of the project as <see cref="Project"/>.</param>
    /// <param name="pipelineId">The pipeline identifier.</param>
    /// <param name="definitionType">The pipeline process type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>An instance of <see cref="Pipeline"/>.</returns>
    Task<Pipeline> GetSinglePipelineForScanAsync(Project project, int pipelineId, PipelineProcessType definitionType, CancellationToken cancellationToken = default);
}