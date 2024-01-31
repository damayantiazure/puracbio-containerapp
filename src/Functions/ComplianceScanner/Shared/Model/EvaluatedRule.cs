using System;
using Rabobank.Compliancy.Core.Rules.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

public class EvaluatedRule
{
    public EvaluatedRule(IRule rule)
    {
        if (rule is null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        Name = rule.Name;
        Description = rule.Description;
        Principles = rule.Principles;
        Link = rule.Link;
    }

    public EvaluatedRule()
    {
    }

    public EvaluatedRule(EvaluatedRule instance)
    {
        if (instance == null)
        {
            throw new ArgumentNullException(nameof(instance));
        }

        Name = instance.Name;
        Description = instance.Description;
        Principles = instance.Principles;
        Link = instance.Link;
        IsSox = instance.IsSox;
        Status = instance.Status;
        Reconcile = instance.Reconcile;
        Item = instance.Item;
        RescanUrl = instance.RescanUrl;
        RegisterDeviationUrl = instance.RegisterDeviationUrl;
        DeleteDeviationUrl = instance.DeleteDeviationUrl;
    }

    public string Name { get; set; }
    public string Description { get; set; }
    public BluePrintPrinciple[] Principles { get; set; }
    public string Link { get; set; }
    public bool IsSox { get; set; }
    public bool Status { get; set; }
    public Reconcile Reconcile { get; set; }
    public Item Item { get; set; }
    public Uri RescanUrl { get; set; }
    public Uri RegisterDeviationUrl { get; set; }
    public Uri DeleteDeviationUrl { get; set; }
}