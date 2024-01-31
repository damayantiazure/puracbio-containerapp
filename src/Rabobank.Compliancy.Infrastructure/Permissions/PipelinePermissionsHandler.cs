using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;

namespace Rabobank.Compliancy.Infrastructure.Permissions;

public class PipelinePermissionsHandler : IPermissionsHandler<Pipeline>
{
    private readonly IPermissionRepository _permissionRepository;

    public PipelinePermissionsHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<PermissionsSet> GetPermissionsForIdentityAsync(Pipeline pipeline, Guid groupId, CancellationToken cancellationToken = default)
    {
        if (pipeline.GetType() == typeof(AzdoBuildDefinitionPipeline))
        {
            return await _permissionRepository.GetBuildDefinitionDisplayPermissionsAsync(
                pipeline.Project.Organization, pipeline.Project.Id, pipeline.Id.ToString(), pipeline.Path, groupId, cancellationToken);
        }

        if (pipeline.GetType() == typeof(AzdoReleaseDefinitionPipeline))
        {
            return await _permissionRepository.GetReleaseDefinitionDisplayPermissionsAsync(
                pipeline.Project.Organization, pipeline.Project.Id, pipeline.Id.ToString(), pipeline.Path, groupId, cancellationToken);
        }

        throw new ArgumentException($"Unsupported resource type: {pipeline.GetType()}");
    }
}