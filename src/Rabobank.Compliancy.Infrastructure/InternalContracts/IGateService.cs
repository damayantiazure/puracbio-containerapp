using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.InternalContracts;

/// <summary>
/// Service definitions for the <see cref="Gate"/>s.
/// </summary>
public interface IGateService
{
    /// <summary>
    /// Retrieves a collection of gates containing check by using the build definition identifier.
    /// </summary>
    /// <param name="project">An instance of the project as <see cref="Project"/>.</param>
    /// <param name="buildDefinitionId">The build definition identifier.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A collection of <see cref="Gate"/>.</returns>
    Task<IEnumerable<Gate>> GetGatesForBuildDefinitionAsync(Project project, int buildDefinitionId, IEnumerable<string> environmentNames, CancellationToken cancellationToken = default);
}