using Rabobank.Compliancy.Core.Rules.Processors;
using Rabobank.Compliancy.Core.Rules.Tests.Resources;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Tests.Processors;

public class RuleProcessorTests
{
    private readonly RuleBuilder _ruleBuilder = new();

    [Fact]
    public void GetAllRules_SameRuleAddedToMultipleCollections_ReturnsDistinctRules()
    {
        // Arrange
        var duplicateRule = _ruleBuilder.GetYamlReleasePipelineRule("dummy");

        var projectRules = new[]{
            _ruleBuilder.GetProjectRule("dummy")
        };
        var buildRules = new[]
        {
            duplicateRule
        };
        var yamlReleaseRules = new[]
        {
            duplicateRule
        };
        var repositoryRules = new[]
        {
            _ruleBuilder.GetRepositoryRule("dummy")
        };
        var classicReleaseRules = new[]
        {
            _ruleBuilder.GetClassicReleasePipelineRule("dummy")
        };

        var ruleProcessor = new RuleProcessor(projectRules, repositoryRules, buildRules, yamlReleaseRules, classicReleaseRules);

        // Act
        var allRules = ruleProcessor.GetAllRules();
        var buildRule = ruleProcessor.GetAllBuildRules().FirstOrDefault();
        var yamlReleaseRule = ruleProcessor.GetAllYamlReleaseRules().FirstOrDefault();

        // Assert
        Assert.Equal(4, allRules.Count());
        Assert.Equal(duplicateRule.Name, buildRule.Name);
        Assert.Equal(duplicateRule.Name, yamlReleaseRule.Name);
    }
}