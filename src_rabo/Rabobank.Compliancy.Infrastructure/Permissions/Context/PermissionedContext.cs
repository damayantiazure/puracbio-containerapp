#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public abstract class PermissionedContext<TProtectedResource> : IPermissionContextForResource<TProtectedResource>
    where TProtectedResource : IProtectedResource
{
    private readonly IPermissionsHandler<TProtectedResource> _handler;
    private TProtectedResource _resource;

    protected PermissionedContext(IPermissionsHandler<TProtectedResource> handler, TProtectedResource resource)
    {
        _handler = handler;
        _resource = resource;
    }

    public TProtectedResource Resource
    {
        get => _resource;
        init
        {
            if (value is not TProtectedResource resource)
            {
                throw new ArgumentException("Invalid resource type", nameof(value));
            }
            _resource = resource;
        }
    }

    public abstract Guid GetSecurityNamespace();

    public abstract IList<int> GetPermissionBitsInScope();

    public abstract IEnumerable<string> GetNativeAzureDevOpsSecurityDisplayNames();

    public abstract List<string> GetRetentionQuery(TimeSpan retentionPeriodInDays);

    public async Task<PermissionsSet?> GetPermissionsForIdentityAsync(Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _handler.GetPermissionsForIdentityAsync(_resource, groupId, cancellationToken);
    }
}