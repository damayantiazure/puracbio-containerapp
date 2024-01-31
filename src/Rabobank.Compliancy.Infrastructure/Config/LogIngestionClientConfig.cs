#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class LogIngestionClientConfig
{
    public string ModelName { get; set; } = null!;
    public string RuleId { get; set; } = null!;
    public string StreamName { get; set; } = null!;
}