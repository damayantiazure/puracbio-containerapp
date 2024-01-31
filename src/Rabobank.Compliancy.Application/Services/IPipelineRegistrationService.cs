using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Registrations;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
/// Used to CRUD registrations through the applicable client.
/// </summary>
public interface IPipelineRegistrationService
{
    /// <summary>
    /// Gets the registrations
    /// </summary>
    /// <param name="project">The project domain object for which the non prod registrations are fetched</param>
    /// <param name="cancellationToken">Cancels the request</param>
    Task<IEnumerable<NonProdPipelineRegistration>> GetNonProdPipelineRegistrationsForProjectAsync(Project project, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get the registration by PipelineId
    /// </summary>
    /// <param name="pipeline">The pipeline domain object for which the registrations are fetched</param>
    /// <param name="cancellationToken">Cancels the request</param>        
    Task<IEnumerable<NonProdPipelineRegistration>> GetNonProdPipelineRegistrationsForPipelineAsync(Pipeline pipeline, CancellationToken cancellationToken = default);
}