#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.Compliancy.Reports;

[ExcludeFromCodeCoverage]
public class CompliancyReport
{
    public string? Id { get; set; }
    public IList<PipelineReport>? UnregisteredPipelines { get; set; }
    public IList<PipelineReport>? RegisteredPipelinesNoProdStage { get; set; }
    public IList<CiReport>? RegisteredConfigurationItems { get; set; }
    public IList<PipelineReport>? RegisteredPipelines { get; set; }
    public IList<ResourceReport>? BuildPipelines { get; set; }
    public IList<ResourceReport>? Repositories { get; set; }
    public IList<NonProdCompliancyReport>? NonProdPipelinesRegisteredForScan { get; set; }
    public DateTime Date { get; set; }
    public Uri? RescanUrl { get; set; }
    public Uri? HasReconcilePermissionUrl { get; set; }
}