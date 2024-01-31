#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class PoisonMessageReport
{
    public string? FailedQueueTrigger { get; set; }
    public string? MessageText { get; set; }
}