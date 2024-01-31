#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker.Model;
using Rabobank.Compliancy.Functions.PipelineBreaker.Services;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Exceptions;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Rabobank.Compliancy.Core.InputValidation.Services;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using ErrorMessages = Rabobank.Compliancy.Functions.PipelineBreaker.Exceptions.ErrorMessages;

namespace Rabobank.Compliancy.Functions.PipelineBreaker;

public class PipelineBreakerFunction
{
    private readonly PipelineBreakerConfig _config;
    private readonly IValidateInputService _validateInputService;
    private readonly IPipelineBreakerService _pipelineBreakerService;
    private readonly IPipelineRegistrationRepository _registrationRepository;
    private readonly IExclusionStorageRepository _exclusionRepository;
    private readonly Application.Services.ILoggingService _loggingService;

    public PipelineBreakerFunction(
        PipelineBreakerConfig config,
        IValidateInputService validateInputService,
        IPipelineBreakerService pipelineBreakerService,
        IPipelineRegistrationRepository registrationRepository,
        IExclusionStorageRepository exclusionRepository,
        Application.Services.ILoggingService loggingService)
    {
        _config = config;
        _validateInputService = validateInputService;
        _pipelineBreakerService = pipelineBreakerService;
        _registrationRepository = registrationRepository;
        _exclusionRepository = exclusionRepository;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(PipelineBreakerFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "pipeline-compliant/{organization}/{projectId}/{runId}/{stageId}/{pipelineType}")]
        HttpRequestMessage request, string organization, string projectId, string runId, string stageId,
        string pipelineType)
    {
        var runInfo = new PipelineRunInfo(organization, projectId, runId, stageId, pipelineType);

        try
        {
            _validateInputService.Validate(organization, projectId, runId, stageId, request);
            _validateInputService.ValidateItemType(pipelineType,
                new[] { ItemTypes.BuildPipeline, ItemTypes.ReleasePipeline });

            // Prod pipelines need to be checked for compliance. In all other cases, if already scanned, function can stop.
            var previousResult = await _pipelineBreakerService.GetPreviousRegistrationResultAsync(runInfo);
            if ((previousResult.Result == PipelineBreakerResult.Passed &&
                 previousResult.RegistrationStatus != PipelineRegistration.Prod) ||
                (previousResult.Result == PipelineBreakerResult.Warned && !_config.BlockUnregisteredPipelinesEnabled))
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
            if (previousComplianceResult.IsValidResult(ExclusionConstants.HoursValid, _config.BlockIncompliantPipelinesEnabled))
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
                Result = PipelineBreakerExtensions.GetResult(validExclusion, ruleReports, _config.ThrowWarningsIncompliantPipelinesEnabled, _config.BlockIncompliantPipelinesEnabled)
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
            try
            {
                var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                    (request, nameof(PipelineBreakerFunction), projectId)
                {
                    Organization = organization,
                    RunId = runId
                };
                await _loggingService.LogExceptionAsync(LogDestinations.PipelineBreakerErrorLog, excp, exceptionBaseMetaInformation,
                    runInfo.PipelineId, pipelineType);

                return new OkObjectResult(ErrorMessages.InternalServerErrorMessage());
            }
            catch
            {
                return new OkObjectResult(ErrorMessages.InternalServerErrorMessage());
            }
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