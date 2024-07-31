using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.RuleProfiles;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.Rules.Processors;

/// <summary>
/// Implementations of this interface are responsible of handling all existing Rule Implementations:
/// - Retrieving
/// - Filtering
/// </summary>
public interface IRuleProcessor
{
    IRule GetRuleByName(string ruleName);
    TRule GetRuleByName<TRule>(string ruleName);
    IEnumerable<IRule> GetAllRules();
    IEnumerable<IProjectRule> GetAllProjectRules();
    IEnumerable<IBuildPipelineRule> GetAllBuildRules();
    IEnumerable<IRepositoryRule> GetAllRepositoryRules();
    IEnumerable<IYamlReleasePipelineRule> GetAllYamlReleaseRules();
    IEnumerable<IClassicReleasePipelineRule> GetAllClassicReleaseRules();
    IEnumerable<IProjectRule> GetProfileProjectRules(RuleProfile profile);
    IEnumerable<IBuildPipelineRule> GetProfileBuildRules(RuleProfile profile);
    IEnumerable<IRepositoryRule> GetProfileRepositoryRules(RuleProfile profile);
    IEnumerable<IYamlReleasePipelineRule> GetProfileYamlReleaseRules(RuleProfile profile);
    IEnumerable<IClassicReleasePipelineRule> GetProfileClassicReleaseRules(RuleProfile profile);
}