#nullable enable

using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;

namespace Rabobank.Compliancy.Application.Services;

/// <summary>
///     Used to manipulate deviations through the applicable client.
/// </summary>
public interface IDeviationService
{
    /// <summary>
    ///     Creates and returns a new Deviation based on the user input. If the deviation already exists, it is replaced.
    /// </summary>
    /// <param name="deviation">Should hold the validated information provided by the user</param>
    /// <param name="username">Used for column <see cref="DeviationEntity.UpdatedBy" /></param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    Task CreateOrReplaceDeviationAsync(Deviation deviation, string username,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the deviation by its primary identifying properties
    /// </summary>
    Task<Deviation?> GetDeviationAsync(Project project, string? ruleName, string? itemId,
        string? ciIdentifier, Guid? foreignProjectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Gets the deviations that belong to the specified project.
    /// </summary>
    Task<IEnumerable<Deviation>> GetDeviationsAsync(Guid projectId, CancellationToken cancellationToken = default);

    /// <summary>
    ///     Deletes the deviation by converting the deviation into an
    /// </summary>
    /// <param name="deviation">Should hold the validated information provided by the user</param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>    
    Task DeleteDeviationAsync(Deviation deviation, CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a deviation insert or delete record
    /// </summary>
    /// <param name="deviation">Should hold the validated information provided by the user</param>    
    Task SendDeviationUpdateRecord(Deviation deviation, DeviationReportLogRecordType recordType);
}