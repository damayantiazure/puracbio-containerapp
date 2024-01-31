using Microsoft.VisualStudio.Services.Operations;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// Provides methods to cater to all object needs from the Azure Devops API regarding
/// </summary>
public interface IOperationRepository
{
    /// <summary>
    /// Get the operation by using the operation identifier.
    /// </summary>
    /// <param name="organization">The organization of the operation</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="Operation"/> representing a Operation the way Azure Devops API returns it.</returns>
    Task<Operation?> GetOperationReferenceByIdAsync(string organization, Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Returns true when the operation at Azure's side is still in progress.
    /// </summary>
    /// <param name="organization">The organization of the operation</param>
    /// <param name="id">The operation identifier.</param>
    /// <param name="cancellationToken">Cancels the API call if necessary.</param>
    /// <returns>Nullable <see cref="Operation"/> representing a Operation the way Azure Devops API returns it.</returns>
    /// <returns></returns>
    Task<bool> OperationIsInProgressAsync(string organization, Guid id, CancellationToken cancellationToken = default);
}