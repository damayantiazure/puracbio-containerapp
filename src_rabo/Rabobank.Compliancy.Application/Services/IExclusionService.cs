#nullable enable
using Rabobank.Compliancy.Domain.Compliancy.Exclusions;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// An interface defining the CRUD for the exclusion table storage.
/// </summary>
public interface IExclusionService
{
    /// <summary>
    /// CreateOrUpdateExclusionAsync will create or update the exclusion record.
    /// </summary>
    /// <param name="exclusion">The exclusion to be created or updated.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a new instance of <see cref="Exclusion"/>.</returns>
    Task<Exclusion?> CreateOrUpdateExclusionAsync(Exclusion exclusion, CancellationToken cancellationToken = default);

    /// <summary>
    /// GetExclusionAsync retrieve the exclusion record using the given parameters.
    /// </summary>
    /// <param name="organization">The name of the organization.</param>
    /// <param name="projectId">The project identifier.</param>
    /// <param name="pipelineId">The pipelien identifier.</param>
    /// <param name="pipelineType">The pipeline type.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a new instance of <see cref="Exclusion"/>.</returns>
    Task<Exclusion?> GetExclusionAsync(string? organization, Guid? projectId, int? pipelineId, string? pipelineType, CancellationToken cancellationToken = default);
}