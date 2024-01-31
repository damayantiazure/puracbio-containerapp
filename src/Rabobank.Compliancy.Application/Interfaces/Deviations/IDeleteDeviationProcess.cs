using Rabobank.Compliancy.Application.Requests;

namespace Rabobank.Compliancy.Application.Interfaces.Deviations;

/// <summary>
/// An interface defining the logic for the delete deviation process.
/// </summary>
public interface IDeleteDeviationProcess
{
    /// <summary>
    /// DeleteDeviationAsync will handle the deletion of a specific deviation.
    /// </summary>
    /// <param name="request">The request <see cref="DeleteDeviationRequest"/> to be handled in the delete deviation process.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents an asynchronous operation as <see cref="Task"/>.</returns>
    Task DeleteDeviationAsync(DeleteDeviationRequest request, CancellationToken cancellationToken = default);
}