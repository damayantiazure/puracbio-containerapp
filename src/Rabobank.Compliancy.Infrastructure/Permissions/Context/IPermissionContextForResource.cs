#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public interface IPermissionContextForResource<TProtectedResource> : IPermissionContext
    where TProtectedResource : IProtectedResource
{
    TProtectedResource Resource { get; init; }

    Task<PermissionsSet?> GetPermissionsForIdentityAsync(Guid groupId, CancellationToken cancellationToken = default);
}