#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class RegistrationPipelineReport
{
    public RegistrationPipelineReport(string ciId, string ciName, string stageId)
    {
        CiId = ciId;
        CiName = ciName;
        StageId = stageId;
    }

    public string? CiId { get; set; }
    public string? CiName { get; set; }
    public string? StageId { get; set; }
}