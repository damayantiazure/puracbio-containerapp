#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure.Permissions;

public class RepositoryPermissionsHandler : IPermissionsHandler<GitRepo>
{
    private readonly IPermissionRepository _permissionRepository;

    public RepositoryPermissionsHandler(IPermissionRepository permissionRepository)
    {
        _permissionRepository = permissionRepository;
    }

    public async Task<PermissionsSet?> GetPermissionsForIdentityAsync(GitRepo gitRepo, Guid groupId, CancellationToken cancellationToken = default)
    {
        return await _permissionRepository.GetRepositoryDisplayPermissionsAsync(gitRepo.Project.Organization, gitRepo.Project.Id, gitRepo.Id, groupId, cancellationToken);
    }
}