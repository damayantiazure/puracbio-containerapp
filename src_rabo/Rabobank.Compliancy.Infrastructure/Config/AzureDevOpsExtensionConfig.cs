#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infrastructure.Config;

[ExcludeFromCodeCoverage]
public class AzureDevOpsExtensionConfig
{
    public string ExtensionName { get; set; } = null!;
}