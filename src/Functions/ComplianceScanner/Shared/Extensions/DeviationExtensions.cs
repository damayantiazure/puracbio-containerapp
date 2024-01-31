using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;

internal static class DeviationExtensions
{
    internal static DeviationReport ToDeviationReport(this Deviation deviation) =>
        new()
        {
            Comment = deviation.Comment,
            Reason = deviation.Reason,
            ReasonNotApplicable = deviation.ReasonNotApplicable,
            ReasonNotApplicableOther = deviation.ReasonNotApplicableOther,
            ReasonOther = deviation.ReasonOther,
            UpdatedBy = deviation.UpdatedBy
        };
}