#nullable enable

using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;

namespace Rabobank.Compliancy.Infrastructure.InternalContracts;

public interface IPermissionGroupService
{
    Task<IEnumerable<Guid>> GetUniqueIdentifiersForNativeAzdoGroups<TProtectedResource>(IPermissionContextForResource<TProtectedResource> context, CancellationToken cancellationToken)
        where TProtectedResource : IProtectedResource;
}