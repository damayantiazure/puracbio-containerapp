#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class RuleCompliancyReport
{
    public string? RuleDescription { get; set; }
    public bool IsCompliant { get; set; }
    public bool HasDeviation { get; set; }
    public string? ItemName { get; set; }

    public override string ToString()
    {
        var compliantOrNotString = IsCompliant ? "compliant" : "incompliant";
        var compliantWithDeviation = HasDeviation && !IsCompliant;
        var compliancyString = compliantWithDeviation ? "compliant with deviation" : compliantOrNotString;
        return $"Rule: '{RuleDescription}' for '{ItemName}' is {compliancyString}. ";
    }

    public bool IsDeterminedCompliant() =>
        IsCompliant || (!IsCompliant && HasDeviation);
}