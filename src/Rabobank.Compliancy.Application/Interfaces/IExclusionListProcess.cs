#nullable enable
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Interfaces;

/// <summary>
/// An interface containing logic to process a Exclusion.
/// </summary>
public interface IExclusionListProcess
{
    /// <summary>
    /// CreateOrUpdateExclusionListAsync will create or update the exclusion record.
    /// </summary>
    /// <param name="exclusionListRequest">The request containing details to process the exclusions.</param>
    /// <param name="User">The user instance.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Returns a <see cref="string"/> with a message to display to the user.</returns>
    Task<string> CreateOrUpdateExclusionListAsync(ExclusionListRequest exclusionListRequest, User user, CancellationToken cancellationToken = default);
}