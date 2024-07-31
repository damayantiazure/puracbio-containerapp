using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class VerifyComplianceService : IVerifyComplianceService
{
    private readonly IEnumerable<BluePrintPrinciple> _principles =
        typeof(BluePrintPrinciples).GetFields().Select(f => (BluePrintPrinciple)f.GetValue(f));

    public IEnumerable<PrincipleReport> CreatePrincipleReports(IEnumerable<EvaluatedRule> evaluatedRules, DateTime scanDate) =>
        _principles.Select(principle => ToPrincipleReport(principle, evaluatedRules, scanDate)).ToArray(); // Materialize collection with ToArray before pass by reference is an option in other parts of the code

    private static PrincipleReport ToPrincipleReport(BluePrintPrinciple principle,
        IEnumerable<EvaluatedRule> evaluatedRules, DateTime scanDate)
    {
        var distinctRules = GetDistinctRules(evaluatedRules);
        var principleRules = GetPrincipleRules(principle, distinctRules);
        var ruleReports = principleRules.Select(rule => new RuleReport(rule.Name, scanDate)
        {
            Description = rule.Description,
            DocumentationUrl = string.IsNullOrWhiteSpace(rule.Link)
                ? null
                : new Uri(rule.Link),
            ItemReports = GetItemReports(rule, evaluatedRules, scanDate).ToArray() // Materialize collection with ToArray before pass by reference is an option in other parts of the code
        }).ToList();

        return new PrincipleReport(principle.Description, scanDate)
        {
            RuleReports = ruleReports,
            HasRulesToCheck = principle.HasRulesToCheck,
            IsSox = principle.IsSox
        };
    }

    private static IEnumerable<EvaluatedRule> GetDistinctRules(IEnumerable<EvaluatedRule> evaluatedRules)
    {
        var rulesGroupedByName = evaluatedRules.GroupBy(rule => rule.Name);
        return rulesGroupedByName.Select(rulesGroup =>
        {
            var firstRuleOfGroup = rulesGroup.First();
            return new EvaluatedRule(firstRuleOfGroup) { Status = IsRulesSetCompliant(rulesGroup.ToList()) };
        });
    }

    private static IEnumerable<ItemReport> GetItemReports(EvaluatedRule evaluatedRule,
        IEnumerable<EvaluatedRule> evaluatedRules, DateTime scanDate)
    {
        var rules = evaluatedRules.Where(r => r.Name == evaluatedRule.Name);
        return rules.Where(rule => rule.Item != null).Select(rule => new ItemReport(
            rule.Item.Id, rule.Item.Name, rule.Item.ProjectId, scanDate)
        {
            IsCompliantForRule = rule.Status,
            ReconcileUrl = rule.Reconcile?.Url,
            ReconcileImpact = rule.Reconcile?.Impact,
            RescanUrl = rule.RescanUrl,
            RegisterDeviationUrl = rule.RegisterDeviationUrl,
            DeleteDeviationUrl = rule.DeleteDeviationUrl,
            Type = rule.Item.Type,
            Link = rule.Item.Link
        });
    }

    private static bool IsRulesSetCompliant(IList<EvaluatedRule> evaluatedRules) =>
        evaluatedRules != null && evaluatedRules.Any() && evaluatedRules.All(r => r.Status);

    private static IEnumerable<EvaluatedRule> GetPrincipleRules(
        BluePrintPrinciple principle, IEnumerable<EvaluatedRule> evaluatedRules) =>
        evaluatedRules.Where(rule => rule.Principles
            .Select(principle => principle.Description)
            .Contains(principle.Description));
}