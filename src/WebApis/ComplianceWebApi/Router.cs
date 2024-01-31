using ComplianceWebApi.Configurations;
using ComplianceWebApi.Services;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Exceptions;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace ComplianceWebApi;

public static class Router
{
    public const string PROJECT_ROUTE = "{projectId}";


    public static WebApplication MapApiRoutes(this WebApplication app)
    {
        var apiGroup = app.MapGroup("api");
        var projectApiRoute = apiGroup.MapGroup(PROJECT_ROUTE).WithOpenApi();

        projectApiRoute.MapPost("pipeline-compliant/{runId}/{stageId}/{pipelineType}", CheckPipelineCompliancyAsync);

        return app;
    }

    private static async Task<IActionResult> CheckPipelineCompliancyAsync(
        [FromRoute] string projectId,
        [FromRoute] string runId,
        [FromRoute] string stageId,
        [FromRoute] string pipelineType,
        [FromServices] PipelineBreakerConfig _config,
        [FromServices] IHttpContextAccessor httpContextAccessor,
        [FromServices] ILoggingService _loggingService,
        [FromServices] IExclusionStorageRepository _exclusionRepository,
        [FromServices] IPipelineBreakerService _pipelineBreakerService,
        [FromServices] IPipelineRegistrationRepository _registrationRepository,
        [FromServices] IValidateInputService _validateInputService,
        [FromServices] AzureDevOpsClientConfig azureDevOpsClientConfig)
    {
        var runInfo = new PipelineRunInfo(azureDevOpsClientConfig.orgName, projectId, runId, stageId, pipelineType);
        try
        {
            //var request = httpContextAccessor.HttpContext;
            //_validateInputService.Validate(azureDevOpsClientConfig.orgName, projectId, runId, stageId, request);
            _validateInputService.ValidateItemType(pipelineType,new[] { ItemTypes.BuildPipeline, ItemTypes.ReleasePipeline });

            // Prod pipelines need to be checked for compliance. In all other cases, if already scanned, function can stop.
            var previousResult = await _pipelineBreakerService.GetPreviousRegistrationResultAsync(runInfo);
            if ((previousResult.Result == PipelineBreakerResult.Passed &&
                 previousResult.RegistrationStatus != PipelineRegistration.Prod) ||
                (previousResult.Result == PipelineBreakerResult.Warned /*TODO : FIX THIS LATER |&& !_config.BlockUnregisteredPipelinesEnabled */))
            {
                return new OkObjectResult(ResultMessages.AlreadyScanned(previousResult.Result));
            }

            runInfo = await _pipelineBreakerService.EnrichPipelineInfoAsync(runInfo);

            var registrations = await _registrationRepository.GetAsync(CreatePipelineRegistrationRequest(runInfo));

            var registrationReport = new PipelineBreakerRegistrationReport
            {
                Date = DateTime.Now,
                Organization = runInfo.Organization,
                ProjectId = runInfo.ProjectId,
                ProjectName = runInfo.ProjectName,
                PipelineId = runInfo.PipelineId,
                PipelineName = runInfo.PipelineName,
                PipelineType = runInfo.PipelineType,
                PipelineVersion = runInfo.PipelineVersion,
                RunId = runInfo.RunId,
                RunUrl = runInfo.RunUrl,
                StageId = runInfo.StageId,
                RegistrationStatus = PipelineBreakerExtensions.GetRegistrationStatus(registrations, runInfo),
                CiIdentifier = registrations?.GetCiIdentifiersDisplayString(),
                CiName = registrations?.GetCiNamesDisplayString()
            };
            registrationReport.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, registrationReport.RegistrationStatus, _config.BlockUnregisteredPipelinesEnabled);

            await _loggingService.LogInformationAsync(LogDestinations.PipelineBreakerLog, registrationReport);

            // Check for registration of pipeline
            // If pipeline is PROD, function continues to verify compliance.
            if (registrationReport.Result == PipelineBreakerResult.Passed &&
                registrationReport.RegistrationStatus != PipelineRegistration.Prod)
            {
                return new OkObjectResult(DecoratorResultMessages.Passed);
            }

            if (registrationReport.Result == PipelineBreakerResult.Warned)
            {
                return new OkObjectResult(ResultMessages.Warned(runInfo.PipelineType, runInfo.ErrorMessage));
            }

            if (registrationReport.Result == PipelineBreakerResult.Blocked)
            {
                return new OkObjectResult(ResultMessages.Blocked(runInfo.PipelineType, runInfo.ErrorMessage));
            }

            // If multiple prod stages are registered, only 1 needs to be scanned for compliance.
            // If RuleCompliancyReports is null, pipeline ran a non-prod stage, and function needs to continue.
            var previousComplianceResult = await _pipelineBreakerService.GetPreviousComplianceResultAsync(runInfo);
            if (previousComplianceResult.IsValidResult(ExclusionConstants.HoursValid, _config.BlockNonCompliantPipelinesEnabled))
            {
                return new OkObjectResult(ResultMessages.AlreadyScanned(previousComplianceResult.Result));
            }

            var exclusion = await _exclusionRepository.GetExclusionAsync(runInfo);
            // Check if exclusion is approved, in valid time window and for current run (or new).
            var validExclusion = exclusion != null && !exclusion.IsExpired(ExclusionConstants.HoursValid) && exclusion.IsApproved
                                 && exclusion.IsNotConsumedOrIsCurrentRun(runInfo.RunId);

            if (validExclusion)
            {
                await _exclusionRepository.SetRunIdAsync(runInfo);
            }

            var ruleReports = await _pipelineBreakerService.GetCompliancy(runInfo, registrations);

            var report = new PipelineBreakerReport
            {
                Date = DateTime.Now,
                Organization = runInfo.Organization,
                ProjectId = runInfo.ProjectId,
                ProjectName = runInfo.ProjectName,
                PipelineId = runInfo.PipelineId,
                PipelineName = runInfo.PipelineName,
                PipelineType = runInfo.PipelineType,
                PipelineVersion = runInfo.PipelineVersion,
                RunId = runInfo.RunId,
                RunUrl = runInfo.RunUrl,
                StageId = runInfo.StageId,
                IsExcluded = validExclusion,
                Requester = exclusion?.Requester,
                ExclusionReasonRequester = exclusion?.ExclusionReasonRequester,
                Approver = exclusion?.Approver,
                ExclusionReasonApprover = exclusion?.ExclusionReasonApprover,
                RuleCompliancyReports = ruleReports,
                CiIdentifier = registrations?.GetCiIdentifiersDisplayString(),
                CiName = registrations?.GetCiNamesDisplayString(),
                Result = PipelineBreakerExtensions.GetResult(validExclusion, ruleReports, 
                _config.ThrowWarningsIncompliantPipelinesEnabled, _config.BlockNonCompliantPipelinesEnabled)
            };

            await _loggingService.LogInformationAsync(LogDestinations.PipelineBreakerComplianceLog, report);

            return new OkObjectResult(ComplianceResultMessages.GetResultMessage(report));
        }
        catch (NoRegisteredStagesFoundException e)
        {
            // We do not want to log this exception, but only inform the user of the message
            return new OkObjectResult(@$"{nameof(DecoratorPrefix.WARNING)}: {e.Message}");
        }
        catch (Exception excp)
        {
            return new OkObjectResult(Configurations.ErrorMessages.InternalServerErrorMessage());
        }
    }

    private static GetPipelineRegistrationRequest CreatePipelineRegistrationRequest(PipelineRunInfo runInfo) =>
    new()
    {
        Organization = runInfo.Organization,
        PipelineId = runInfo.PipelineId,
        PipelineType = runInfo.PipelineType,
        ProjectId = runInfo.ProjectId
    };
}
