#nullable enable

using System.Collections.Generic;
using System.Linq;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;

internal static class StageExtensions
{
    private static StageReport ToStageReport(this Stage stage) =>
        new()
        {
            Id = stage.Id,
            Name = stage.Name
        };

    internal static IEnumerable<StageReport>? ToStageReports(this IEnumerable<Stage>? stages) =>
        stages?.Select(s => s.ToStageReport());
}