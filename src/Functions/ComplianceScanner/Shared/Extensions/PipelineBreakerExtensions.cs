using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient.Exceptions;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;


namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
public static class PipelineBreakerExtensions
{
    public static string GetRegistrationStatus(IEnumerable<PipelineRegistration> registrations,
        PipelineRunInfo runInfo)
    {
        if (!registrations.Any())
        {
            return null;
        }

        if (!registrations.Any(pipelineRegistration => pipelineRegistration.IsProduction))
        {
            return PipelineRegistration.NonProd;
        }

        var registeredProdStages = registrations.Where(pipelineRegistration => pipelineRegistration.IsProduction)
            .Select(pipelineRegistration => pipelineRegistration.StageId)
            .Distinct(StringComparer.OrdinalIgnoreCase);

        var prodStages = runInfo.Stages?.Select(stage => stage.Id)
            .Where(text => registeredProdStages.Contains(text, StringComparer.OrdinalIgnoreCase));

        if (!prodStages.Any())
        {
            throw new NoRegisteredStagesFoundException(DecoratorResultMessages.NoProdStagesFound);
        }

        return PipelineRegistration.Prod;
    }

    public static PipelineBreakerResult GetResult(
        string pipelineType, string registrationStatus, bool isBlockingEnabled)
    {
        if (registrationStatus != null ||
            pipelineType == ItemTypes.ClassicBuildPipeline ||
            pipelineType == ItemTypes.StagelessYamlPipeline ||
            pipelineType == ItemTypes.YamlPipelineWithStages)
        {
            return PipelineBreakerResult.Passed;
        }

        // All other cases warn or block (so also if pipelineType == ItemTypes.InvalidYamlPipeline)
        return isBlockingEnabled
            ? PipelineBreakerResult.Blocked
            : PipelineBreakerResult.Warned;
    }

    public static PipelineBreakerResult GetResult(bool isExcluded,
        IEnumerable<RuleCompliancyReport> reports, bool throwWarnings, bool isBlockingEnabled)
    {
        if (isExcluded ||
            !throwWarnings && !isBlockingEnabled)
        {
            return PipelineBreakerResult.Passed;
        }

        // Pipeline is not running prod stage
        if (!reports.Any())
        {
            return PipelineBreakerResult.Passed;
        }

        if (reports.All(ruleCompliancyReport => ruleCompliancyReport.IsDeterminedCompliant()))
        {
            return PipelineBreakerResult.Passed;
        }

        return (isBlockingEnabled)
            ? PipelineBreakerResult.Blocked
            : PipelineBreakerResult.Warned;
    }
}
