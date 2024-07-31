using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Core.Rules.Model;

public class BluePrintPrinciple
{
    public BluePrintPrinciple(string description, bool hasRulesToCheck, bool isSox)
    {
        Description = description;
        HasRulesToCheck = hasRulesToCheck;
        IsSox = isSox;
    }

    [ExcludeFromCodeCoverage] public string Description { get; set; }
    [ExcludeFromCodeCoverage] public bool HasRulesToCheck { get; set; }
    [ExcludeFromCodeCoverage] public bool IsSox { get; set; }
}