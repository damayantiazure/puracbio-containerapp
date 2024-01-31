#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class LogIngestionConfig
{
    public Uri EndPoint { get; set; } = null!;
    public IEnumerable<LogIngestionClientConfig> Clients { get; set; } = null!;
}