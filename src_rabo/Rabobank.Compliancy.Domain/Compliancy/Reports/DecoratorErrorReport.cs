#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class DecoratorErrorReport
{
    public string? Organization { get; set; }
    public string? ProjectId { get; set; }
    public string? RunId { get; set; }
    public string? ReleaseId { get; set; }
    public string? PipelineType { get; set; }
    public string? StageName { get; set; }
    public string? Message { get; set; }
}