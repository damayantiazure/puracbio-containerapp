using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Application.Interfaces.OpenPermissions;
using Rabobank.Compliancy.Application.OpenPermissions;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;

public static class FunctionDependencies
{
    public static IServiceCollection AddFunctionDependencies(this IServiceCollection services)
    {
        services
            .AddScoped<IOpenGitRepoPermissionsProcess, OpenGitRepoPermissionsProcess>()
            .AddScoped<IOpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline>, OpenPipelinePermissionsProcess<AzdoBuildDefinitionPipeline>>()
            .AddScoped<IOpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline>, OpenPipelinePermissionsProcess<AzdoReleaseDefinitionPipeline>>()
        ;

        return services;
    }
}