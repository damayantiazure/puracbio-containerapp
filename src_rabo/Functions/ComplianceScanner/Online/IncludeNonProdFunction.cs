#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Rabobank.Compliancy.Core.InputValidation.Services;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class IncludeNonProdFunction
{
    private readonly IPipelineRegistrator _registrator;
    private readonly IValidateInputService _validateInputService;
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;
    private readonly Application.Services.ILoggingService _loggingService;

    public IncludeNonProdFunction(
        IPipelineRegistrator registrator,
        IValidateInputService validateInputService,
        ICheckAuthorizationProcess checkAuthorizationProcess,
        Application.Services.ILoggingService loggingService)
    {
        _registrator = registrator;
        _validateInputService = validateInputService;
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(IncludeNonProdFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "includenonprod/{organization}/{projectId}/{pipelineId}/{pipelineType}")]
        HttpRequestMessage request, string organization, Guid projectId,
        string pipelineId, string pipelineType)
    {
        var stageId = await GetStageIdFromBodyAsync(request);

        try
        {
            _validateInputService.Validate(organization, projectId.ToString(), pipelineId, request);
            _validateInputService.ValidateItemType(pipelineType,
                new[] { ItemTypes.YamlReleasePipeline, ItemTypes.ClassicReleasePipeline });

            var authorizationRequest = new AuthorizationRequest(projectId, organization);

            if (!await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, request.Headers.Authorization))
            {
                return new UnauthorizedResult();
            }

            return await _registrator.UpdateNonProdRegistrationAsync(organization, projectId.ToString(), pipelineId, pipelineType, stageId);
        }
        catch (ArgumentException ex)
        {
            var exceptionBaseMetaInformation = await LogExceptionAsync(ex);
            return new BadRequestObjectResult(
                $"{ex.Message} (CorrelationId:{exceptionBaseMetaInformation.CorrelationId})");
        }
        catch (Exception ex)
        {
            await LogExceptionAsync(ex);
            throw;
        }

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception ex)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (nameof(IncludeNonProdFunction), organization, projectId.ToString(), request.RequestUri?.AbsoluteUri);
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex, pipelineId, stageId, null);
            return exceptionBaseMetaInformation;
        }
    }

    private static async Task<string?> GetStageIdFromBodyAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return null;
        }

        var content = await request.Content.ReadAsStringAsync();
        if (content == string.Empty)
        {
            return null;
        }

        dynamic data = JsonConvert.DeserializeObject(content)!;

        if (string.IsNullOrEmpty((string?)data.environment))
        {
            return null;
        }

        return (string)data.environment;
    }
}