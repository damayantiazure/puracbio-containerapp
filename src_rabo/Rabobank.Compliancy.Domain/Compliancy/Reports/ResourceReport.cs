#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class ResourceReport
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Link { get; set; }
    public bool IsProduction { get; set; }
    public string? CiIdentifiers { get; set; }
    public Uri? OpenPermissionsUrl { get; set; }
    public Uri? DocumentationUrl { get; set; }
}