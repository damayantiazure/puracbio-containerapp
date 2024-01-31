#nullable enable

using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class VerifyPrincipleCompliancyActivityTests
{
    [Fact]
    public void EvaluatedRulesStateShouldNotBeMutated()
    {
        // Arrange
        var rules = new[]
        {
            new EvaluatedRule
            {
                Name = "Rule1",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Status = true,
                Item = new Item
                    {Id = "0", Name = "Multistage pipeline 1", Type = ItemTypes.YamlReleasePipeline}
            },
            new EvaluatedRule
            {
                Name = "Rule1",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Status = false,
                Item = new Item
                    {Id = "1", Name = "Multistage pipeline 2", Type = ItemTypes.YamlReleasePipeline}
            }
        };

        // Act
        var service = new VerifyComplianceService();
        var result = service.CreatePrincipleReports(rules, DateTime.Now);

        // Assert
        var principleReport = result.First(x => x.Name == BluePrintPrinciples.FourEyes.Description);
        var itemReports = principleReport!.RuleReports!.First(r => r.Name == "Rule1").ItemReports;
        itemReports!.Count().ShouldBe(2);
        itemReports!.Any(r => r.IsCompliantForRule).ShouldBe(true);
        itemReports!.Any(r => r.IsCompliantForRule == false).ShouldBe(true);
    }

    [Theory, InlineData("Enforce 4 eyes for every change to production", "Rule1", "0", "YAML release"),
     InlineData("Enforce 4 eyes for every change to production", "Rule2", "1", "Classic release")]
    public void ItemsShouldBeAddedToCorrectRules(
        string bluePrintPrincipleDescription, string ruleName, string itemId, string itemType)
    {
        // Arrange
        var rules = new[]
        {
            new EvaluatedRule
            {
                Name = "Rule1",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Status = true,
                Item = new Item
                    {Id = "0", Name = "Multistage pipeline 1", Type = ItemTypes.YamlReleasePipeline}
            },
            new EvaluatedRule
            {
                Name = "Rule2",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Status = true,
                Item = new Item
                    {Id = "1", Name = "Release pipeline 1", Type = ItemTypes.ClassicReleasePipeline}
            }
        };

        // Act
        var service = new VerifyComplianceService();
        var result = service.CreatePrincipleReports(rules, DateTime.Now);

        // Assert
        var principleReport = result.First(x => x.Name == bluePrintPrincipleDescription);
        var itemReport = principleReport!.RuleReports!.FirstOrDefault(r => r.Name == ruleName)?
            .ItemReports!.FirstOrDefault(r => r.ItemId == itemId);
        itemReport.ShouldNotBeNull();
        itemReport.Type.ShouldBe(itemType);
    }

    [Fact]
    public void ShouldIncludeRuleReportsInCompliancyReport()
    {
        // arrange
        var rules = new List<EvaluatedRule>
        {
            new EvaluatedRule
            {
                Name = "YamlReleasePipelineIsBlockedWithout4EyesApproval",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Description = "Production deployment is blocked without 4-eyes approval",
                Status = false,
                Link = "http://test1.nl"
            },
            new EvaluatedRule
            {
                Name = "NobodyCanOverrideGateOutcomesAndStartDeployments",
                Principles = new[] {BluePrintPrinciples.FourEyes}
            },
            new EvaluatedRule
            {
                Name = "ClassicReleasePipelineIsBlockedWithout4EyesApproval",
                Principles = new[] {BluePrintPrinciples.FourEyes},
                Description = "Production deployment is blocked without 4-eyes approval",
                Status = true,
                Link = "http://test2.nl"
            }
        };

        // Act
        var service = new VerifyComplianceService();
        var result = service.CreatePrincipleReports(rules, DateTime.Now);

        // assert
        var report =
            result.FirstOrDefault(x => x.Name == BluePrintPrinciples.FourEyes.Description);
        report.ShouldNotBeNull();
        report.RuleReports!
            .FirstOrDefault(r => r.Name == "YamlReleasePipelineIsBlockedWithout4EyesApproval")
            .ShouldNotBeNull();
        report.RuleReports!
            .FirstOrDefault(r => r.Name == "NobodyCanOverrideGateOutcomesAndStartDeployments")
            .ShouldNotBeNull();
        report.RuleReports!
            .FirstOrDefault(r => r.Name == "ClassicReleasePipelineIsBlockedWithout4EyesApproval")
            .ShouldNotBeNull();

        var rule1 = report.RuleReports!.FirstOrDefault(r =>
            r.Name == "YamlReleasePipelineIsBlockedWithout4EyesApproval");
        rule1.ShouldNotBeNull();
        rule1.Description.ShouldNotBe(null);
        rule1.IsCompliant.ShouldBe(false); // No itemreports so this should be false
        rule1.DocumentationUrl!.ToString().ShouldBe("http://test1.nl/");

        var rule2 = report.RuleReports!.FirstOrDefault(r =>
            r.Name == "ClassicReleasePipelineIsBlockedWithout4EyesApproval");
        rule2.ShouldNotBeNull();
        rule2.Description.ShouldNotBe(null);
        rule2.IsCompliant.ShouldBe(false); // No itemreports so this should be false
        rule2.DocumentationUrl!.ToString().ShouldBe("http://test2.nl/");
    }
}