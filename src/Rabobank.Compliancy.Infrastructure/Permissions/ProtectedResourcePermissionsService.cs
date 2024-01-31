#nullable enable

using AutoMapper;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;

namespace Rabobank.Compliancy.Infrastructure.Permissions;

public class ProtectedResourcePermissionsService : IProtectedResourcePermissionsService
{
    private readonly ILogQueryService _logQueryService;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMapper _mapper;
    private readonly IPermissionGroupService _permissionGroupService;
    private readonly IPermissionContextFactory _contextFactory;

    public ProtectedResourcePermissionsService(ILogQueryService logQueryService, IPermissionRepository permissionRepository, IMapper mapper, IPermissionGroupService permissionGroupService, IPermissionContextFactory contextFactory)
    {
        _logQueryService = logQueryService;
        _permissionRepository = permissionRepository;
        _mapper = mapper;
        _permissionGroupService = permissionGroupService;
        _contextFactory = contextFactory;
    }

    public async Task<DeploymentInformation?> GetProductionDeploymentAsync<TProtectedResource>(IProtectedResource protectedResource, TimeSpan releasePipelineRetentionPeriodInDays, CancellationToken cancellationToken = default)
        where TProtectedResource : IProtectedResource
    {
        var context = _contextFactory.CreateContext<TProtectedResource>(protectedResource);

        var retentionQueries = context.GetRetentionQuery(releasePipelineRetentionPeriodInDays);

        DeploymentInformation? deploymentInformation = null;
        foreach (var retentionQuery in retentionQueries)
        {
            deploymentInformation = await _logQueryService.GetQueryEntryAsync<DeploymentInformation>(retentionQuery, cancellationToken);
            if (deploymentInformation != null)
            {
                break;
            }
        }

        return deploymentInformation;
    }

    public async Task OpenPermissionedResourceAsync<TProtectedResource>(IProtectedResource protectedResource, CancellationToken cancellationToken = default)
        where TProtectedResource : IProtectedResource
    {
        var context = _contextFactory.CreateContext<TProtectedResource>(protectedResource);

        var resource = context.Resource;
        var organization = resource.Project.Organization;
        var projectId = resource.Project.Id;

        var nativeAzdoGroups = await _permissionGroupService
            .GetUniqueIdentifiersForNativeAzdoGroups(context, cancellationToken);

        foreach (var groupId in nativeAzdoGroups)
        {
            var allPermissionsForIdentity = await context.GetPermissionsForIdentityAsync(groupId, cancellationToken);
            if (allPermissionsForIdentity == null)
            {
                continue;
            }

            var permissionsInScope = allPermissionsForIdentity.Permissions?
                .Where(permission => context.GetPermissionBitsInScope()
                    .Contains(permission.PermissionBit))
                .ToList();

            if (permissionsInScope == null)
            {
                continue;
            }

            foreach (var permission in permissionsInScope)
            {
                permission.PermissionId = (int)PermissionType.Allow;

                var content = CreateUpdatePermissionBodyContent(groupId, permission, allPermissionsForIdentity);

                await _permissionRepository.UpdatePermissionGroupAsync(organization, projectId, content,
                    cancellationToken);
            }
        }
    }

    private UpdatePermissionBodyContent CreateUpdatePermissionBodyContent(Guid groupId, Clients.AzureDevopsClient.AzdoRequests.Permission.Models.Permission permission, PermissionsSet allPermissionsForIdentity)
    {
        var permissionEntity = _mapper.Map<UpdatePermissionEntity>(permission);

        var updatePermissionBody = _mapper.Map<UpdatePermissionBody>(allPermissionsForIdentity);
        updatePermissionBody.PermissionSetToken = permission.PermissionToken;
        updatePermissionBody.TeamFoundationId = groupId;
        updatePermissionBody.Updates.Add(permissionEntity);

        return new UpdatePermissionBodyContent { UpdatePackage = JsonConvert.SerializeObject(updatePermissionBody) };
    }
}