#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Deviations;

public class Deviation
{
    [SuppressMessage("Sonar Code Smell",
        "S107: Constructor has 9 parameters, which is greater than the 7 authorized.",
        Justification = "Passing arguments by constructor to make entities immutable is a best practice.")]
    public Deviation(string itemId, string ruleName, string ciIdentifier, Project project, DeviationReason? reason,
        DeviationApplicabilityReason? reasonNotApplicable, string? reasonOther, string? reasonNotApplicableOther,
        string comment)
    {
        ItemId = itemId;
        RuleName = ruleName;
        CiIdentifier = ciIdentifier;
        Project = project;
        Reason = reason;
        ReasonNotApplicable = reasonNotApplicable;
        ReasonOther = reasonOther;
        ReasonNotApplicableOther = reasonNotApplicableOther;
        Comment = comment;
    }

    public string ItemId { get; }
    public string RuleName { get; }
    public string CiIdentifier { get; }

    /// <summary>
    ///     Project where registration of this deviation takes place
    /// </summary>
    public Project Project { get; }

    // Identifying properties (What rule and item is this deviation for)
    public Guid? ItemProjectId { get; set; }

    // Deviation details (Describes why this deviation is registered)
    public string? Comment { get; }
    public DeviationReason? Reason { get; }
    public DeviationApplicabilityReason? ReasonNotApplicable { get; }
    public string? ReasonNotApplicableOther { get; }
    public string? ReasonOther { get; }
    public string? UpdatedBy { get; set; }
}