using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Core.Rules.Rules;

namespace Rabobank.Compliancy.Core.Rules;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDefaultRules(this IServiceCollection collection) =>
        collection
            .AddGlobalPermissions()
            .AddRepositoryRules()
            .AddBuildRules()
            .AddClassicReleaseRules()
            .AddYamlReleaseRules()
            .AddRuleProcessor();

    public static IServiceCollection AddGlobalPermissions(this IServiceCollection collection) =>
        collection
            .AddSingleton<IProjectRule, NobodyCanDeleteTheProject>()
            .AddSingleton<IProjectReconcile, NobodyCanDeleteTheProject>();

    public static IServiceCollection AddRepositoryRules(this IServiceCollection collection) =>
        collection
            .AddSingleton<IRepositoryRule, NobodyCanDeleteTheRepository>()
            .AddSingleton<IReconcile, NobodyCanDeleteTheRepository>();

    public static IServiceCollection AddBuildRules(this IServiceCollection collection) =>
        collection
            .AddSingleton<IBuildPipelineRule, NobodyCanDeleteBuilds>()
            .AddSingleton<IReconcile, NobodyCanDeleteBuilds>()
            .AddSingleton<IBuildPipelineRule, BuildArtifactIsStoredSecure>()
            .AddSingleton<IBuildPipelineRule, BuildPipelineHasSonarqubeTask>()
            .AddSingleton<IBuildPipelineRule, BuildPipelineHasFortifyTask>()
            .AddSingleton<IBuildPipelineRule, BuildPipelineHasNexusIqTask>()
            .AddSingleton<IBuildPipelineRule, BuildPipelineHasCredScanTask>()
            .AddSingleton<IBuildPipelineRule, BuildPipelineFollowsMainframeCobolProcess>();

    public static IServiceCollection AddClassicReleaseRules(this IServiceCollection collection) =>
        collection
            .AddSingleton<IClassicReleasePipelineRule, NobodyCanDeleteReleases>()
            .AddSingleton<IReconcile, NobodyCanDeleteReleases>()
            .AddSingleton<IClassicReleasePipelineRule, NobodyCanManagePipelineGatesAndDeploy>()
            .AddSingleton<IReconcile, NobodyCanManagePipelineGatesAndDeploy>()
            .AddSingleton<IClassicReleasePipelineRule, ClassicReleasePipelineHasRequiredRetentionPolicy>()
            .AddSingleton<IReconcile, ClassicReleasePipelineHasRequiredRetentionPolicy>()
            .AddSingleton<IClassicReleasePipelineRule, ClassicReleasePipelineUsesBuildArtifact>()
            .AddSingleton<IClassicReleasePipelineRule, ClassicReleasePipelineHasSm9ChangeTask>()
            .AddSingleton<IClassicReleasePipelineRule, ClassicReleasePipelineIsBlockedWithout4EyesApproval>()
            .AddSingleton<IReconcile, ClassicReleasePipelineIsBlockedWithout4EyesApproval>()
            .AddSingleton<IClassicReleasePipelineRule, ClassicReleasePipelineFollowsMainframeCobolReleaseProcess>();

    public static IServiceCollection AddYamlReleaseRules(this IServiceCollection collection) =>
        collection
            .AddSingleton<IYamlReleasePipelineRule, NobodyCanDeleteBuilds>()
            .AddSingleton<IReconcile, NobodyCanDeleteBuilds>()
            .AddSingleton<IYamlReleasePipelineRule, YamlReleasePipelineIsBlockedWithout4EyesApproval>()
            .AddSingleton<IReconcile, YamlReleasePipelineIsBlockedWithout4EyesApproval>()
            .AddSingleton<IYamlReleasePipelineRule, YamlReleasePipelineHasRequiredRetentionPolicy>()
            .AddSingleton<IReconcile, YamlReleasePipelineHasRequiredRetentionPolicy>()
            .AddSingleton<IYamlReleasePipelineRule, YamlReleasePipelineHasSm9ChangeTask>()
            .AddSingleton<IYamlReleasePipelineRule, NobodyCanManageEnvironmentGatesAndDeploy>()
            .AddSingleton<IReconcile, NobodyCanManageEnvironmentGatesAndDeploy>()
            .AddSingleton<IYamlReleasePipelineRule, YamlReleasePipelineFollowsMainframeCobolReleaseProcess>();

    public static IServiceCollection AddRuleProcessor(this IServiceCollection collection) =>
        collection
            .AddSingleton<IRuleProcessor, RuleProcessor>();
}