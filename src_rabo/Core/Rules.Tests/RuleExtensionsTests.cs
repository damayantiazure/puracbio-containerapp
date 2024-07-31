using AutoFixture;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Tests.Resources;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Tests;

public class RuleExtensionsTests
{
    private readonly RuleBuilder _ruleBuilder = new();
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void GetAllByRuleProfile_ReturnsOnlyRulesInProfile()
    {
        // set up
        var dummyRuleName1 = _fixture.Create<string>();
        var dummyRuleName2 = _fixture.Create<string>();
        var dummyRuleName3 = _fixture.Create<string>();

        var ruleProfile = _ruleBuilder.CreateRuleProfile(new string[] { dummyRuleName1, dummyRuleName2 });

        var dummyRule1 = _ruleBuilder.GetBuildPipelineRule(dummyRuleName1);
        var dummyRule2 = _ruleBuilder.GetYamlReleasePipelineRule(dummyRuleName2);
        var dummyRule3 = _ruleBuilder.GetRepositoryRule(dummyRuleName3);

        List<IRule> allRules = new() { dummyRule1, dummyRule2, dummyRule3 };

        // do
        var filteredrules = allRules.GetAllByRuleProfile(ruleProfile);

        // assert
        Assert.Equal(2, filteredrules.Count());
        Assert.Equal(3, allRules.Count);
        Assert.Contains(dummyRule1, filteredrules);
        Assert.Contains(dummyRule2, filteredrules);
        Assert.DoesNotContain(dummyRule3, filteredrules);
    }


}