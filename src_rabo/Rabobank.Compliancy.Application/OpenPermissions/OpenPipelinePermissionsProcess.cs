#nullable enable

using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.Requests.OpenPermissions;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Application.OpenPermissions;

/// <inheritdoc/>
public class OpenPipelinePermissionsProcess<TPipeline>
    : OpenProtectedResourcePermissionsProcess<OpenPipelinePermissionsRequest<TPipeline>, TPipeline>,
    IOpenPipelinePermissionsProcess<TPipeline>
    where TPipeline : Pipeline
{
    private readonly IProjectService _projectService;
    private readonly IPipelineService _pipelineService;

    public OpenPipelinePermissionsProcess(IProtectedResourcePermissionsService permissionsService, IProjectService projectService, IPipelineService pipelineService)
        : base(permissionsService)
    {
        _projectService = projectService;
        _pipelineService = pipelineService;
    }

    protected override async Task<IProtectedResource> GetProtectedResource(OpenPipelinePermissionsRequest<TPipeline> openPermissionRequest, CancellationToken cancellationToken)
    {
        var project = await _projectService.GetProjectByIdAsync(openPermissionRequest.Organization, openPermissionRequest.ProjectId, cancellationToken);
        return await _pipelineService.GetPipelineAsync<TPipeline>(project, openPermissionRequest.PipelineId, cancellationToken);
    }
}