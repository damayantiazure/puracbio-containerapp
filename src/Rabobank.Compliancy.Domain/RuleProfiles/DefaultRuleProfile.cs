using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Domain.RuleProfiles;

public class DefaultRuleProfile : RuleProfile
{
    private readonly IEnumerable<string> _ruleNames = new[]
    {
        RuleNames.BuildArtifactIsStoredSecure,
        RuleNames.BuildPipelineHasCredScanTask,
        RuleNames.BuildPipelineHasFortifyTask,
        RuleNames.BuildPipelineHasNexusIqTask,
        RuleNames.BuildPipelineHasSonarqubeTask,
        RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy,
        RuleNames.ClassicReleasePipelineHasSm9ChangeTask,
        RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval,
        RuleNames.ClassicReleasePipelineUsesBuildArtifact,
        RuleNames.NobodyCanDeleteBuilds,
        RuleNames.NobodyCanDeleteReleases,
        RuleNames.NobodyCanDeleteTheProject,
        RuleNames.NobodyCanDeleteTheRepository,
        RuleNames.NobodyCanManageEnvironmentGatesAndDeploy,
        RuleNames.NobodyCanManagePipelineGatesAndDeploy,
        RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy,
        RuleNames.YamlReleasePipelineHasSm9ChangeTask,
        RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval
    };

    public override Profiles Profile { get { return Profiles.Default; } }

    public override IEnumerable<string> Rules { get { return _ruleNames; } }
}