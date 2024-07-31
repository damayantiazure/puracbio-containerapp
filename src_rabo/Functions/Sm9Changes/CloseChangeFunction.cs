#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Helpers;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes;

public class CloseChangeFunction
{
    private readonly ICloseChangeProcess _closeChangeProcess;
    private readonly ISm9ChangesService _sm9ChangesService;
    private readonly ILoggingService _loggingService;

    public CloseChangeFunction(
        ILoggingService loggingService,
        ISm9ChangesService sm9ChangesService,
        ICloseChangeProcess closeChangeProcess)
    {
        _sm9ChangesService = sm9ChangesService;
        _loggingService = loggingService;
        _closeChangeProcess = closeChangeProcess;
    }

    [FunctionName(nameof(CloseChangeFunction))]
    public async Task<IActionResult> RunAsync(
        [HttpTrigger(AuthorizationLevel.Function, WebRequestMethods.Http.Post,
            Route = "close-change/{organization}/{projectId}/{pipelineType}/{runId}")]
        HttpRequestMessage request, string? organization, Guid projectId, string? pipelineType, int runId)
    {
        try
        {
            _sm9ChangesService.ValidateFunctionInput(request, organization, projectId, pipelineType, runId);

            var closeChangeDetails = await GetUserInputAsync(request);

            var closeRequest = new CloseChangeRequest
            {
                Organization = organization,
                ProjectId = projectId,
                PipelineType = pipelineType,
                RunId = runId,
                CloseChangeDetails = closeChangeDetails
            };

            var (validChangeIds, alreadyClosed) = await _closeChangeProcess.CloseChangeAsync(closeRequest);

            return CreateResponseHelper.CreateResponseTask(
                ResponseMessages.SuccessfullyClosed(validChangeIds, alreadyClosed), true);
        }
        catch (Exception ex) when (
            ex is InvalidUserInputException
                or ChangeIdNotFoundException
                or ChangePhaseValidationException
                or ArgumentException)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return CreateResponseHelper.CreateResponseTask(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})", false);
        }
        catch (ChangeClientException ex)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return CreateResponseHelper.CreateResponseTask(ErrorMessages.Sm9TeamError(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})"), false);
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (request, nameof(CloseChangeFunction), projectId.ToString())
            {
                Organization = organization,
                RunId = runId.ToString(),
                PipelineType = pipelineType
            };

            await _loggingService.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog, exceptionBaseMetaInformation, e);
            return exceptionBaseMetaInformation;
        }
    }

    private static async Task<CloseChangeDetails> GetUserInputAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            throw new InvalidOperationException("Cannot retrieve content from request.");
        }

        var content = await request.Content.ReadAsStringAsync();
        var input = JsonConvert.DeserializeObject<CloseChangeDetails>(content);
        if (input == null)
        {
            throw new InvalidOperationException("Cannot retrieve content from request.");
        }

        if (!IsValidCompletionCode(input.CompletionCode))
        {
            throw new InvalidUserInputException(ErrorMessages.InvalidCompletionCode(null, true, input.CompletionCode));
        }

        return input;
    }

    private static bool IsValidCompletionCode(string? completionCode) =>
        !string.IsNullOrEmpty(completionCode) && SM9Constants.CompletionCode.Contains(completionCode);
}