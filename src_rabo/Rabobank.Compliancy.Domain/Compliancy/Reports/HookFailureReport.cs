#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class HookFailureReport
{
    public DateTime Date { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ErrorDetail { get; set; }
    public string? Organization { get; set; }
    public Guid? ProjectId { get; set; }
    public string? PipelineId { get; set; }
    public string? HookId { get; set; }
    public string? EventId { get; set; }
    public string? EventType { get; set; }
    public string? EventResourceData { get; set; }
}