#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class StageReport
{
    public string? Id { get; set; }
    public string? Name { get; set; }
}