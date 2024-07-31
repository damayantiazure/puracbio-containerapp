# nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Permissions;

public interface IPermissionsHandler<in TProtectedResource> where TProtectedResource : IProtectedResource
{
    Task<PermissionsSet?> GetPermissionsForIdentityAsync(TProtectedResource resource, Guid groupId, CancellationToken cancellationToken = default);
}