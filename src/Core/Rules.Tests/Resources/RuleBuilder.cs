using Moq;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.RuleProfiles;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Tests.Resources;

internal class RuleBuilder
{
    private readonly Mock<IBuildPipelineRule> _buildPipelineRule = new();
    private readonly Mock<IYamlReleasePipelineRule> _yamlReleasePipelineRule = new();
    private readonly Mock<IClassicReleasePipelineRule> _classicReleasePipelineRule = new();
    private readonly Mock<IRepositoryRule> _repositoryRule = new();
    private readonly Mock<IProjectRule> _projectRule = new();
    private readonly Mock<RuleProfile> _ruleProfile = new();

    public RuleBuilder() { }

    public RuleProfile CreateRuleProfile(string[] rulenames)
    {
        _ruleProfile.Setup(p => p.Rules).Returns(rulenames);
        return _ruleProfile.Object;
    }

    public IBuildPipelineRule GetBuildPipelineRule(string name)
    {
        _buildPipelineRule.SetupGet(m => m.Name).Returns(name);
        return _buildPipelineRule.Object;
    }
    public IEnumerable<IBuildPipelineRule> GetBuildPipelineRules(string[] names)
    {
        return names.Select(name => GetBuildPipelineRule(name));
    }

    public IClassicReleasePipelineRule GetClassicReleasePipelineRule(string name)
    {
        _classicReleasePipelineRule.SetupGet(m => m.Name).Returns(name);
        return _classicReleasePipelineRule.Object;
    }

    public IYamlReleasePipelineRule GetYamlReleasePipelineRule(string name)
    {
        _yamlReleasePipelineRule.SetupGet(m => m.Name).Returns(name);
        return _yamlReleasePipelineRule.Object;
    }

    public IRepositoryRule GetRepositoryRule(string name)
    {
        _repositoryRule.SetupGet(m => m.Name).Returns(name);
        return _repositoryRule.Object;
    }

    public IProjectRule GetProjectRule(string name)
    {
        _projectRule.SetupGet(m => m.Name).Returns(name);
        return _projectRule.Object;
    }
}