#nullable enable

using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Models;

namespace Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;

[ExcludeFromCodeCoverage]
public class CompliancyReportDto : ExtensionData
{
    [JsonProperty("unregisteredPipelines")]
    public IList<PipelineReportDto>? UnregisteredPipelines { get; set; }

    [JsonProperty("registeredPipelinesNoProdStage")]
    public IList<PipelineReportDto>? RegisteredPipelinesNoProdStage { get; set; }

    [JsonProperty("registeredConfigurationItems")]
    public IList<CiReportDto>? RegisteredConfigurationItems { get; set; }

    [JsonProperty("registeredPipelines")]
    public IList<PipelineReportDto>? RegisteredPipelines { get; set; }

    [JsonProperty("buildPipelines")]
    public IList<ResourceReportDto>? BuildPipelines { get; set; }

    [JsonProperty("repositories")]
    public IList<ResourceReportDto>? Repositories { get; set; }

    [JsonProperty("nonProdPipelinesRegisteredForScan")]
    public IList<NonProdCompliancyReportDto>? NonProdPipelinesRegisteredForScan { get; set; }

    [JsonProperty("date")]
    public DateTime Date { get; set; }

    [JsonProperty("rescanUrl")]
    public Uri? RescanUrl { get; set; }

    [JsonProperty("hasReconcilePermissionUrl")]
    public Uri? HasReconcilePermissionUrl { get; set; }
}