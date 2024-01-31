#nullable enable

using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.OpenPermissions;

/// <inheritdoc/>
public class OpenGitRepoPermissionsProcess : OpenProtectedResourcePermissionsProcess<OpenGitRepoPermissionsRequest, GitRepo>, IOpenGitRepoPermissionsProcess
{
    private readonly IProjectService _projectService;
    private readonly IGitRepoService _gitRepoService;

    public OpenGitRepoPermissionsProcess(IProtectedResourcePermissionsService permissionsService, IProjectService projectService, IGitRepoService gitRepoService)
        : base(permissionsService)
    {
        _gitRepoService = gitRepoService;
        _projectService = projectService;
    }

    protected override async Task<IProtectedResource> GetProtectedResource(OpenGitRepoPermissionsRequest openPermissionRequest, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(openPermissionRequest.Organization, openPermissionRequest.ProjectId, cancellationToken);
        return await _gitRepoService.GetGitRepoByIdAsync(project, openPermissionRequest.GitRepoId, cancellationToken);
    }
}