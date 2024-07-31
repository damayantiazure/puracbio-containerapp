using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Helpers;

public static class CreateUrl
{
    public static Uri ItemRescanUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string ruleName, string itemId,
        string foreignProjectId = null) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/scan/" +
            $"{organization}/{projectId}/{ruleName}/{itemId}/{foreignProjectId}");

    public static Uri CiRescanUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string ciIdentifier) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/scan/" +
            $"{organization}/{projectId}/{ciIdentifier}");

    public static Uri NonProdPipelineRescanUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/scanpipeline/" +
            $"{organization}/{projectId}/{pipelineId}");

    public static Uri ProjectRescanUrl(ComplianceConfig environmentConfig,
        string organization, string projectId) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/scan/" +
            $"{organization}/{projectId}");

    public static Uri RegisterUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId, string pipelineType) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/register/" +
            $"{organization}/{projectId}/{pipelineId}/{pipelineType}");

    public static Uri UpdateRegistrationUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId, string pipelineType) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/updateregistration/" +
            $"{organization}/{projectId}/{pipelineId}/{pipelineType}");

    public static Uri DeleteRegistrationUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId, string pipelineType) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/deleteregistration/" +
            $"{organization}/{projectId}/{pipelineId}/{pipelineType}");

    public static Uri RegisterDeviationUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string ciIdentifier, string ruleName,
        string itemId, string foreignProjectId = null) =>
        ciIdentifier.IsProduction()
            ? new($"https://{environmentConfig.OnlineScannerHostName}/api/register-deviation/" +
                  $"{organization}/{projectId}/{ciIdentifier}/{ruleName}/{itemId}/{foreignProjectId}")
            : null;

    public static Uri DeleteDeviationUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string ciIdentifier, string ruleName,
        string itemId, string foreignProjectId = null) =>
        ciIdentifier.IsProduction()
            ? new($"https://{environmentConfig.OnlineScannerHostName}/api/delete-deviation/" +
                  $"{organization}/{projectId}/{ciIdentifier}/{ruleName}/{itemId}/{foreignProjectId}")
            : null;

    public static Uri OpenPermissionsUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string itemType, string itemId) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/open-permissions/" +
            $"{organization}/{projectId}/{itemType}/{itemId}");

    public static Uri HasPermissionUrl(ComplianceConfig environmentConfig,
        string organization, string projectId) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/" +
            $"has-permissions/{organization}/{projectId}");

    public static Uri ExclusionListUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId, string pipelineType) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/" +
            $"exclusion-list/{organization}/{projectId}/{pipelineId}/{pipelineType}");

    public static Uri InvalidPipelineDocumentationUrl() =>
        new(ConfluenceLinks.InvalidPipelines);

    public static Uri AddNonProdPipelineToScanUrl(ComplianceConfig environmentConfig,
        string organization, string projectId, string pipelineId, string pipelineType) =>
        new($"https://{environmentConfig.OnlineScannerHostName}/api/includenonprod/" +
            $"{organization}/{projectId}/{pipelineId}/{pipelineType}");

    public static Reconcile ReconcileFromRule(ComplianceConfig environmentConfig, string organization,
        string projectId, string itemId, IProjectReconcile rule) =>
        rule == null
            ? null
            : new Reconcile
            {
                Url = new Uri($"https://{environmentConfig.OnlineScannerHostName}/api/" +
                              $"reconcile/{organization}/{projectId}/{rule.Name}/{itemId}"),
                Impact = rule.Impact
            };

    public static Reconcile ReconcileFromRule(ComplianceConfig environmentConfig, string organization,
        string projectId, string itemId, IReconcile rule) =>
        rule == null
            ? null
            : new Reconcile
            {
                Url = new Uri($"https://{environmentConfig.OnlineScannerHostName}/api/" +
                              $"reconcile/{organization}/{projectId}/{rule.Name}/{itemId}"),
                Impact = rule.Impact
            };
}