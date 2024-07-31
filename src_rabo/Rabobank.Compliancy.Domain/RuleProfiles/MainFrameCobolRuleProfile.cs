using Rabobank.Compliancy.Domain.Rules;

namespace Rabobank.Compliancy.Domain.RuleProfiles;

public class MainFrameCobolRuleProfile : RuleProfile
{
    private readonly IEnumerable<string> _ruleNames = new[]
    {
        RuleNames.ClassicReleasePipelineHasRequiredRetentionPolicy,
        RuleNames.ClassicReleasePipelineHasSm9ChangeTask,
        RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval,
        RuleNames.NobodyCanDeleteBuilds,
        RuleNames.NobodyCanDeleteReleases,
        RuleNames.NobodyCanDeleteTheProject,
        RuleNames.NobodyCanDeleteTheRepository,
        RuleNames.NobodyCanManageEnvironmentGatesAndDeploy,
        RuleNames.NobodyCanManagePipelineGatesAndDeploy,
        RuleNames.YamlReleasePipelineHasRequiredRetentionPolicy,
        RuleNames.YamlReleasePipelineHasSm9ChangeTask,
        RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval,
        RuleNames.BuildPipelineFollowsMainframeCobolProcess,
        RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess,
        RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess,
        RuleNames.BuildPipelineHasSonarqubeTask
    };

    public override Profiles Profile => Profiles.MainframeCobol;

    public override IEnumerable<string> Rules => _ruleNames;
}