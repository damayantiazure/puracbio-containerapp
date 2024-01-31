using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;
public interface IPermissionContextFactory
{
    IPermissionContextForResource<TProtectedResource> CreateContext<TProtectedResource>(IProtectedResource protectedResource)
        where TProtectedResource : IProtectedResource;
}