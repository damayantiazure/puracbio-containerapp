#nullable enable

using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Constants;
using Rabobank.Compliancy.Domain.Exceptions;

namespace Rabobank.Compliancy.Application.OpenPermissions;

/// <inheritdoc/>
public abstract class OpenProtectedResourcePermissionsProcess<TOpenPermissionRequest, TProtectedResource>
    : IOpenProtectedResourcePermissionsProcess<TOpenPermissionRequest, TProtectedResource>
    where TProtectedResource : IProtectedResource
    where TOpenPermissionRequest : OpenPermissionsRequestBase<TProtectedResource>
{
    protected readonly IProtectedResourcePermissionsService _permissionsService;

    protected OpenProtectedResourcePermissionsProcess(IProtectedResourcePermissionsService permissionsService)
    {
        _permissionsService = permissionsService;
    }

    /// <inheritdoc/>
    public async Task OpenPermissionAsync(TOpenPermissionRequest request, CancellationToken cancellationToken = default)
    {
        var protectedResource = await GetProtectedResource(request, cancellationToken);

        await CheckForResourceRetentionException(protectedResource, cancellationToken);

        await OpenPermissions(protectedResource, cancellationToken);
    }

    private async Task CheckForResourceRetentionException(IProtectedResource resource, CancellationToken cancellationToken)
    {
        var productionDeployment = await _permissionsService.GetProductionDeploymentAsync<TProtectedResource>(resource, PipelineConstants.ReleasePipelineRetentionPeriodInDays, cancellationToken);

        if (productionDeployment != null)
        {
            throw new IsProductionItemException(productionDeployment.CompletedOn.ToString(),
                productionDeployment.CiName, productionDeployment.RunUrl);
        }
    }

    protected abstract Task<IProtectedResource> GetProtectedResource(TOpenPermissionRequest request, CancellationToken cancellationToken);

    private async Task OpenPermissions(IProtectedResource protectedResource, CancellationToken cancellationToken)
    {
        await _permissionsService.OpenPermissionedResourceAsync<TProtectedResource>(protectedResource, cancellationToken);
    }
}