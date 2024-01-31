using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace ComplianceWebApi.Configurations;

public record class PipelineBreakerConfig(
    bool BlockUnregisteredPipelinesEnabled,
    bool BlockNonCompliantPipelinesEnabled,
    bool BlockNonCompliantProdPipelinesEnabled,
    bool BlockNonCompliantNonProdPipelinesEnabled,
    bool ThrowWarningsIncompliantPipelinesEnabled);


public record AzureDevOpsClientConfig(
    string orgName,
    bool useManagedIdentity, string clientIdOfManagedIdentity, string tenantIdOfManagedIdentity,
    bool useServicePrincipal, string clientIdOfServicePrincipal, string clientSecretOfServicePrincipal, string tenantIdOfServicePrincipal,
    bool usePat, string Pat);


public static class LogNames
{
    public const string RegistrationLogName = "pipeline_breaker_log";
    public const string ComplianceLogName = "pipeline_breaker_compliance_log";
    public const string ErrorLogName = "pipeline_breaker_error_log";
    public const string LogTimeField = "createdDate";
}

public static class ErrorMessages
{
    public static string InternalServerErrorMessage() =>
        @$"{DecoratorErrors.ErrorPrefix}An internal server error occurred while executing the compliance scan for this pipeline run. 
As this pipeline run could not be validated, it is allowed to continue.";

    public static string BuildNotAvailableErrorMessage(string runId) =>
        $"The pipelinebreaker application account does not have the correct permissions to retrieve build {runId} " +
        "or the build does not exist.";

    public static string ReleaseNotAvailableErrorMessage(string releaseId) =>
        $"The pipelinebreaker application account does not have the correct permissions to retrieve release {releaseId} " +
        "or the release does not exist.";
}


[ExcludeFromCodeCoverage]
public static class CompliancyScannerExtensionConstants
{
    public const string Publisher = "tas";
    public const string Collection = "compliancy";
}


public static class ResultMessages
{
    public static string Warned(string pipelineType, string errorMessage) =>
        Message(pipelineType, errorMessage, true);

    public static string Blocked(string pipelineType, string errorMessage) =>
        Message(pipelineType, errorMessage, false);

    private static string Message(string pipelineType, string errorMessage, bool warning)
    {
        var errorType = warning ? nameof(DecoratorPrefix.WARNING) : nameof(DecoratorPrefix.ERROR);

        if (pipelineType == ItemTypes.InvalidYamlPipeline)
        {
            return @$"{errorType}: {DecoratorResultMessages.InvalidYaml}
Error message: {errorMessage} ";
        }

        return $"{errorType}: {DecoratorResultMessages.NotRegistered}";
    }

    public static string AlreadyScanned(PipelineBreakerResult? result) =>
        result == PipelineBreakerResult.Passed
            ? DecoratorResultMessages.AlreadyScanned
            : DecoratorResultMessages.WarningAlreadyScanned;
}



public static class ComplianceResultMessages
{
    public static string GetResultMessage(PipelineBreakerReport report)
    {
        if (report.IsExcluded)
        {
            return DecoratorResultMessages.ExclusionList;
        }

        switch (report.Result)
        {
            case PipelineBreakerResult.Passed:
                return DecoratorResultMessages.Passed;

            case PipelineBreakerResult.Warned:
                return $"{DecoratorResultMessages.WarningNotCompliant}{Environment.NewLine}{ConstructCompliancyReportMessage(report.RuleCompliancyReports)}";

            case PipelineBreakerResult.Blocked:
                return $"{DecoratorResultMessages.NotCompliant}{Environment.NewLine}{ConstructCompliancyReportMessage(report.RuleCompliancyReports)}";

            default:
                return string.Empty;
        }
    }

    private static string ConstructCompliancyReportMessage(IEnumerable<RuleCompliancyReport> reports)
    {
        StringBuilder reportString = new();
        foreach (var report in reports.Where(r => !r.IsDeterminedCompliant()))
        {
            reportString.AppendLine(report.ToString());
        }

        if (!reports.All(r => r.IsDeterminedCompliant()))
        {
            reportString.AppendLine($"For more information on how to become compliant, visit: {ConfluenceLinks.CompliancyDocumentation} ");
        }

        return reportString.ToString();
    }
}