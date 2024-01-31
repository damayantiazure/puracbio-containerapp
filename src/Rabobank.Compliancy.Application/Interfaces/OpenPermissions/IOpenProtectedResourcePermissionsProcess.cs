#nullable enable

using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Interfaces.OpenPermissions;

/// <summary>
/// A process definition for opening the build or release pipeline permissions for a specific project.
/// </summary>
public interface IOpenProtectedResourcePermissionsProcess<TOpenPermissionRequest, TProtectedResource>
    where TOpenPermissionRequest : OpenPermissionsRequestBase<TProtectedResource>
    where TProtectedResource : IProtectedResource
{
    /// <summary>
    /// OpenPermissionAsync method will perform the necessary steps to open the resource permissions.
    /// </summary>
    /// <param name="request">The specified request for opening the resource permissions <see cref="OpenPermissionsRequestBase<TProtectedResource>"/>.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>Represents an asynchronous operation as <see cref="Task"/>.</returns>
    Task OpenPermissionAsync(TOpenPermissionRequest request, CancellationToken cancellationToken = default);
}