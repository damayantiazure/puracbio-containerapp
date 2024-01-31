#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class LogConfig
{
    public string WorkspaceId { get; set; } = null!;
    public LogIngestionConfig Ingestion { get; set; } = null!;
}