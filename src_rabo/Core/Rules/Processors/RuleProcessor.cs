using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.RuleProfiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Processors;

/// <inheritdoc/>
/// <summary>
/// This class handles rules by getting them provided at construction time through Dependency Injection
/// This class is intended to be instanciated as a Singleton during startup
/// </summary>
public class RuleProcessor : IRuleProcessor
{
    private readonly IEnumerable<IProjectRule> _projectRules;
    private readonly IEnumerable<IRepositoryRule> _repositoryRules;
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly IEnumerable<IYamlReleasePipelineRule> _yamlReleasePipelineRules;
    private readonly IEnumerable<IClassicReleasePipelineRule> _classicReleasePipelineRules;

    public RuleProcessor(
        IEnumerable<IProjectRule> projectRules,
        IEnumerable<IRepositoryRule> repositoryRules,
        IEnumerable<IBuildPipelineRule> buildPipelineRules,
        IEnumerable<IYamlReleasePipelineRule> yamlReleasePipelineRules,
        IEnumerable<IClassicReleasePipelineRule> classicReleasePipelineRules)
    {
        _projectRules = projectRules;
        _repositoryRules = repositoryRules;
        _buildPipelineRules = buildPipelineRules;
        _yamlReleasePipelineRules = yamlReleasePipelineRules;
        _classicReleasePipelineRules = classicReleasePipelineRules;
    }

    public IEnumerable<IBuildPipelineRule> GetAllBuildRules()
    {
        return _buildPipelineRules;
    }

    public IEnumerable<IClassicReleasePipelineRule> GetAllClassicReleaseRules()
    {
        return _classicReleasePipelineRules;
    }

    public IEnumerable<IProjectRule> GetAllProjectRules()
    {
        return _projectRules;
    }

    public IEnumerable<IRepositoryRule> GetAllRepositoryRules()
    {
        return _repositoryRules;
    }

    public IEnumerable<IYamlReleasePipelineRule> GetAllYamlReleaseRules()
    {
        return _yamlReleasePipelineRules;
    }

    public IEnumerable<IBuildPipelineRule> GetProfileBuildRules(RuleProfile profile)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IClassicReleasePipelineRule> GetProfileClassicReleaseRules(RuleProfile profile)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IProjectRule> GetProfileProjectRules(RuleProfile profile)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IRepositoryRule> GetProfileRepositoryRules(RuleProfile profile)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<IYamlReleasePipelineRule> GetProfileYamlReleaseRules(RuleProfile profile)
    {
        throw new NotImplementedException();
    }

    public IRule GetRuleByName(string ruleName)
    {
        var rule = GetAllRules().FirstOrDefault(rule => rule.Name == ruleName);
        if (rule == default)
        {
            throw new ArgumentOutOfRangeException(nameof(ruleName));
        }
        return rule;
    }

    public TRule GetRuleByName<TRule>(string ruleName)
    {
        var rule = GetRuleByName(ruleName);
        if (rule is not TRule)
        {
            throw new InvalidOperationException($"Rule {ruleName} cannot be cast to {typeof(TRule)}.");
        }

        return (TRule)rule;
    }

    public IEnumerable<IRule> GetAllRules()
    {
        var rules = new List<IRule>();
        rules.AddRange(_projectRules);
        rules.AddRange(_repositoryRules);
        rules.AddRange(_buildPipelineRules);
        rules.AddRange(_yamlReleasePipelineRules);
        rules.AddRange(_classicReleasePipelineRules);
        return rules.Distinct();
    }
}