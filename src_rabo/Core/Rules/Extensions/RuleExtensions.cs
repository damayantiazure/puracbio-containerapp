using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Domain.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Rabobank.Compliancy.Core.Rules.Extensions;

public static class RuleExtensions
{
    // Nullpropagation used for unit tests purposes
    public static IEnumerable<TRule> GetAllByRuleProfile<TRule>(this IEnumerable<TRule> rules, RuleProfile ruleProfile) where TRule : IRule =>
        rules.Where(rule => ruleProfile.Rules.Contains(rule?.Name));
}