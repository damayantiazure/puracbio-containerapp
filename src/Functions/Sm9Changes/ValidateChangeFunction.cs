# nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using Rabobank.Compliancy.Functions.Sm9Changes.Helpers;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes;

public class ValidateChangeFunction
{
    private const int _validateChangeTimeOut = 1;
    private readonly ILoggingService _loggingService;
    private readonly ISm9ChangesService _sm9ChangesService;
    private readonly Dictionary<string, IAzdoService> _azdoService;

    public ValidateChangeFunction(
        IAzdoRestClient azdoClient,
        ILoggingService loggingService,
        ISm9ChangesService sm9ChangesService)
    {
        _loggingService = loggingService;
        _sm9ChangesService = sm9ChangesService;
        _azdoService = new Dictionary<string, IAzdoService>
        {
            [SM9Constants.BuildPipelineType] = new AzdoBuildService(azdoClient),
            [SM9Constants.ReleasePipelineType] = new AzdoReleaseService(azdoClient,
                new AzdoBuildService(azdoClient))
        };
    }

    [FunctionName(nameof(ValidateChangeFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-change")]
        HttpRequestMessage request)
    {
        var organization = string.Empty;
        var projectId = string.Empty;

        try
        {
            ValidateHeaders(request, out organization, out projectId,
                out var buildId, out var releaseId);

            // If a release is triggered by a build, both ReleaseId and BuildId are valid input via the headers.
            // In this case the pipeline has the type: Release Pipeline.
            var (runId, pipelineType) = releaseId.IsValidPipelineRunId()
                ? (releaseId, SM9Constants.ReleasePipelineType)
                : (buildId, SM9Constants.BuildPipelineType);
            var azdoService = _azdoService[pipelineType];

            var isLowRiskChange = await azdoService.IsLowRiskChangeAsync(
                organization, projectId, runId);
            if (isLowRiskChange)
            {
                return CreateResponseHelper.CreateResponseGate(ResponseMessages.LowRiskChange, true);
            }

            var changeIds = await GetChangeIdsAsync(organization, projectId, runId, azdoService, request);

            var changeDetails = await _sm9ChangesService.ValidateChangesAsync(
                changeIds, new[] { SM9Constants.DeploymentPhase, SM9Constants.ExecutionPhase }, _validateChangeTimeOut);

            var invalidChanges = changeDetails.Where(changeInformation => !changeInformation.HasCorrectPhase);

            if (invalidChanges.Any())
            {
                throw new ChangePhaseValidationException(ErrorMessages.InvalidChangePhase(
                    invalidChanges, pipelineType, false, request));
            }

            return CreateResponseHelper.CreateResponseGate(ResponseMessages.CorrectPhase(changeIds), true);
        }
        catch (ChangeClientException e)
        {
            return CreateResponseHelper.CreateResponseGate(ErrorMessages.Sm9TeamError(e.Message, request),
                false);
        }
        catch (Exception ex) when (
            ex is InvalidHeadersException or ChangeIdNotFoundException or ChangePhaseValidationException)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return CreateResponseHelper.CreateResponseGate(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})", false);
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception ex)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (request, nameof(ValidateChangeFunction), projectId)
            {
                Organization = organization
            };

            var exceptionReport = new ExceptionReport(exceptionBaseMetaInformation, ex);
            await _loggingService.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog, exceptionReport);

            return exceptionBaseMetaInformation;
        }
    }

    private static void ValidateHeaders(HttpRequestMessage request, out string organization,
        out string projectId, out string buildId, out string releaseId)
    {
        var organizationUri = request.Headers.PlanUrl();
        if (string.IsNullOrWhiteSpace(organizationUri))
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("PlanUrl", "$(system.CollectionUri)"),
                null, false, request));
        }

        organization = organizationUri.GetAzdoOrganizationName();
        if (string.IsNullOrWhiteSpace(organization))
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("PlanUrl", "$(system.CollectionUri)",
                    organizationUri), null, false, request));
        }

        projectId = request.Headers.ProjectId();
        buildId = request.Headers.BuildId();
        releaseId = request.Headers.ReleaseId();
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("ProjectId", "$(system.TeamProjectId)"),
                null, false, request));
        }

        if (string.IsNullOrWhiteSpace(buildId))
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("BuildId", "$(Build.BuildId)"),
                null, false, request));
        }

        if (string.IsNullOrWhiteSpace(releaseId))
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("ReleaseId", "$(Release.ReleaseId)"),
                null, false, request));
        }

        if (!buildId.IsValidPipelineRunId() && !releaseId.IsValidPipelineRunId())
        {
            throw new InvalidHeadersException(ErrorMessages.InvalidHeader(
                ErrorMessages.InvalidHeaderException("BuildId", "$(Build.BuildId)", "ReleaseId",
                    "$(Release.ReleaseId)"), null, false, request));
        }
    }

    private static async Task<IEnumerable<string>> GetChangeIdsAsync(string organization,
        string projectId, string runId, IAzdoService azdoService, HttpRequestMessage request)
    {
        var changeIdFromVariable = await azdoService.GetChangeIdFromVariableAsync(
            organization, projectId, runId);
        if (changeIdFromVariable != null)
        {
            return new[] { changeIdFromVariable };
        }

        var changeIdsFromTags = await azdoService.GetChangeIdsFromTagsAsync(
            organization, projectId, runId, SM9Constants.ChangeIdRegex);
        if (changeIdsFromTags == null || !changeIdsFromTags.Any())
        {
            throw new ChangeIdNotFoundException(ErrorMessages.ChangeIdNotFound(null, false, request));
        }

        return changeIdsFromTags;
    }
}