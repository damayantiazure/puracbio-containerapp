#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Rabobank.Compliancy.Core.InputValidation.Model;
using ExceptionBaseMetaInformation = Rabobank.Compliancy.Domain.Exceptions.ExceptionBaseMetaInformation;

namespace Rabobank.Compliancy.Functions.ValidateGates;

public class ValidateApproversYamlReleaseFunction
{
    private readonly Core.InputValidation.Services.IValidateInputService _validateInputService;
    private readonly IAzdoRestClient _azdoRestClient;
    private readonly Application.Services.ILoggingService _loggingService;

    public ValidateApproversYamlReleaseFunction(Core.InputValidation.Services.IValidateInputService validateInputService, IAzdoRestClient azdoRestClient
        , Application.Services.ILoggingService loggingService)
    {
        _validateInputService = validateInputService;
        _azdoRestClient = azdoRestClient;
        _loggingService = loggingService;
    }

    [SuppressMessage("Sonar Code Smell",
        "S4457: Split this method into two, one handling parameters check and the other handling the asynchronous code.",
        Justification = "We will allow this since this is hard to implement because of the asynchronous code in the catch blocks. The benefits do not justify the effort.")]
    [FunctionName(nameof(ValidateApproversYamlReleaseFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-yaml-approvers/{projectId}/{runId}/{smokeTestOrganization?}")]
        HttpRequestMessage request, string? projectId, string? runId, string? smokeTestOrganization,
        [DurableClient] IDurableOrchestrationClient durableClient)
    {
        try
        {
            var organizationUri = request.Headers.GetValues("PlanUrl").FirstOrDefault();
            var input = _validateInputService.ValidateInput(projectId, runId, organizationUri, false);
            if (input.GetType() != typeof(OkObjectResult))
            {
                return input;
            }

            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (runId == null)
            {
                throw new ArgumentNullException(nameof(runId));
            }

            var okResult = (OkObjectResult)input;
            var organization = okResult.Value?.ToString();
            if (organization == null)
            {
                throw new ArgumentException($"{nameof(organization)} cannot be null");
            }

            Guid? planId = null;
            if (Guid.TryParse(request.Headers.GetValues("PlanId").FirstOrDefault(), out var planIdentifier))
            {
                planId = planIdentifier;
            }

            // StageId only relevant for production runs. For SmokeTests the StageId is always the same.
            var stageId = smokeTestOrganization != null
                ? "Production"
                : await GetStageIdAsync(organization, projectId, runId, planId);

            var azdoData = new ValidateApproversAzdoData(request, organization, smokeTestOrganization, projectId, runId,
                stageId, null);

            await durableClient.StartNewAsync(nameof(ValidateApproversOrchestrator), azdoData);

            return new OkResult();
        }
        catch (ArgumentException e)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(e);
            return new BadRequestObjectResult(
                ErrorMessages.CreateArgumentExceptionErrorMessage(
                    $"{e.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})"));
        }
        catch (Exception e)
        {
            await LogExceptionAsync(e);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (request, nameof(ValidateApproversYamlReleaseFunction), projectId)
            {
                RunId = runId
            };

            await _loggingService.LogExceptionAsync(LogDestinations.ValidateGatesErrorLog, exceptionBaseMetaInformation, e);
            return exceptionBaseMetaInformation;
        }
    }

    private async Task<string?> GetStageIdAsync(string organization, string projectId, string runId, Guid? planId)
    {
        var timeline = await _azdoRestClient.GetAsync(Builds.Timeline(projectId, runId), organization);
        var records = timeline?.Records;
        var checkPoint = records?.FirstOrDefault(r => r.Id == planId && r.Type == "Checkpoint");
        var stage = records?.FirstOrDefault(r => r.Id == checkPoint?.ParentId && r.Type == "Stage");
        return stage?.Identifier;
    }
}