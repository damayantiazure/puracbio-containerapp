#nullable enable
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.Services;

public interface IProtectedResourcePermissionsService
{
    Task OpenPermissionedResourceAsync<TProtectedResource>(IProtectedResource protectedResource, CancellationToken cancellationToken = default)
        where TProtectedResource : IProtectedResource;

    Task<DeploymentInformation?> GetProductionDeploymentAsync<TProtectedResource>(IProtectedResource protectedResource,
        TimeSpan releasePipelineRetentionPeriodInDays, CancellationToken cancellationToken = default)
        where TProtectedResource : IProtectedResource;
}