namespace Rabobank.Compliancy.Domain.Rules;

public static class RuleNames
{
    public const string BuildArtifactIsStoredSecure = "BuildArtifactIsStoredSecure";
    public const string BuildPipelineHasCredScanTask = "BuildPipelineHasCredScanTask";
    public const string BuildPipelineHasFortifyTask = "BuildPipelineHasFortifyTask";
    public const string BuildPipelineHasNexusIqTask = "BuildPipelineHasNexusIqTask";
    public const string BuildPipelineHasSonarqubeTask = "BuildPipelineHasSonarqubeTask";
    public const string BuildPipelineFollowsMainframeCobolProcess = "BuildPipelineFollowsMainframeCobolProcess";
    public const string ClassicReleasePipelineHasRequiredRetentionPolicy = "ClassicReleasePipelineHasRequiredRetentionPolicy";
    public const string ClassicReleasePipelineHasSm9ChangeTask = "ClassicReleasePipelineHasSm9ChangeTask";
    public const string ClassicReleasePipelineIsBlockedWithout4EyesApproval = "ClassicReleasePipelineIsBlockedWithout4EyesApproval";
    public const string ClassicReleasePipelineUsesBuildArtifact = "ClassicReleasePipelineUsesBuildArtifact";
    public const string ClassicReleasePipelineFollowsMainframeCobolReleaseProcess = "ClassicReleasePipelineFollowsMainframeCobolReleaseProcess";
    public const string NobodyCanDeleteBuilds = "NobodyCanDeleteBuilds";
    public const string NobodyCanDeleteReleases = "NobodyCanDeleteReleases";
    public const string NobodyCanDeleteTheProject = "NobodyCanDeleteTheProject";
    public const string NobodyCanDeleteTheRepository = "NobodyCanDeleteTheRepository";
    public const string NobodyCanManageEnvironmentGatesAndDeploy = "NobodyCanManageEnvironmentGatesAndDeploy";
    public const string NobodyCanManagePipelineGatesAndDeploy = "NobodyCanManagePipelineGatesAndDeploy";
    public const string YamlReleasePipelineFollowsMainframeCobolReleaseProcess = "YamlReleasePipelineFollowsMainframeCobolReleaseProcess";
    public const string YamlReleasePipelineHasRequiredRetentionPolicy = "YamlReleasePipelineHasRequiredRetentionPolicy";
    public const string YamlReleasePipelineHasSm9ChangeTask = "YamlReleasePipelineHasSm9ChangeTask";
    public const string YamlReleasePipelineIsBlockedWithout4EyesApproval = "YamlReleasePipelineIsBlockedWithout4EyesApproval";
}