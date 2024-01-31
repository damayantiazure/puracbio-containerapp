#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ValidateGates;

public class ValidateApproversClassicReleaseFunction
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly Core.InputValidation.Services.IValidateInputService _validateInputService;
    private readonly Application.Services.ILoggingService _loggingService;

    public ValidateApproversClassicReleaseFunction(IAzdoRestClient azdoClient, Core.InputValidation.Services.IValidateInputService validateInputService
        , Application.Services.ILoggingService loggingService)
    {
        _azdoClient = azdoClient;
        _validateInputService = validateInputService;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(ValidateApproversClassicReleaseFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "validate-classic-approvers/{projectId}/{releaseId}/{smokeTestOrganization?}")]
        HttpRequestMessage request, string? projectId, string? releaseId, string? smokeTestOrganization,
        [DurableClient] IDurableOrchestrationClient durableClient)
    {
        try
        {
            var organizationUri = request.Headers.GetValues("PlanUrl").FirstOrDefault();
            var input = _validateInputService.ValidateInput(projectId, releaseId, organizationUri, true);

            if (input.GetType() != typeof(OkObjectResult))
            {
                return input;
            }

            var okResult = (OkObjectResult)input;
            var organization = okResult.Value?.ToString();

            var release = await _azdoClient.GetAsync(ReleaseManagement.Release(projectId, releaseId),
                smokeTestOrganization ?? organization);
            if (release == null)
            {
                return new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
                    $"The release could not be retrieved." +
                    $"Please check your {nameof(projectId)}: '{projectId}' and {nameof(releaseId)}: '{releaseId}'."));
            }

            var azdoData = new ValidateApproversAzdoData(request, organization, smokeTestOrganization, projectId, null,
                null, release);

            await durableClient.StartNewAsync(nameof(ValidateApproversOrchestrator), azdoData);

            return new OkResult();
        }
        catch (ArgumentException e)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(e);
            return new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
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
                (request, nameof(ValidateApproversClassicReleaseFunction), projectId)
            {
                ReleaseId = releaseId
            };

            await _loggingService.LogExceptionAsync(LogDestinations.ValidateGatesErrorLog, exceptionBaseMetaInformation, e);
            return exceptionBaseMetaInformation;
        }
    }
}