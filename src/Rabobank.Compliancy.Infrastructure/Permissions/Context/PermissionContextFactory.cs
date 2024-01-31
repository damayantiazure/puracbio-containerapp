#nullable enable

using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;

namespace Rabobank.Compliancy.Infrastructure.Permissions.Context;

public class PermissionContextFactory : IPermissionContextFactory
{
    private readonly IPermissionsHandler<Pipeline> _pipelinePermissionsHandler;
    private readonly IPermissionsHandler<GitRepo> _repositoryPermissionsHandler;

    public PermissionContextFactory(
        IPermissionsHandler<Pipeline> pipelinePermissionsHandler,
        IPermissionsHandler<GitRepo> repositoryPermissionsHandler)
    {
        _pipelinePermissionsHandler = pipelinePermissionsHandler;
        _repositoryPermissionsHandler = repositoryPermissionsHandler;
    }

    public IPermissionContextForResource<TProtectedResource> CreateContext<TProtectedResource>(IProtectedResource protectedResource)
        where TProtectedResource : IProtectedResource
    {
        if (typeof(TProtectedResource) == typeof(AzdoBuildDefinitionPipeline))
        {
            return (IPermissionContextForResource<TProtectedResource>)new BuildPermissionedContext(_pipelinePermissionsHandler, (AzdoBuildDefinitionPipeline)protectedResource);
        }

        if (typeof(TProtectedResource) == typeof(AzdoReleaseDefinitionPipeline))
        {
            return (IPermissionContextForResource<TProtectedResource>)new ReleasePermissionedContext(_pipelinePermissionsHandler, (AzdoReleaseDefinitionPipeline)protectedResource);
        }

        if (typeof(TProtectedResource) == typeof(GitRepo))
        {
            return (IPermissionContextForResource<TProtectedResource>)new GitRepoPermissionedContext(_repositoryPermissionsHandler, (GitRepo)protectedResource);
        }

        throw new ArgumentException($"Unsupported resource type: {typeof(TProtectedResource)}");
    }
}