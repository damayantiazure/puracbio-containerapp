#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using ErrorMessages = Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions.ErrorMessages;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;
using Project = Rabobank.Compliancy.Infra.AzdoClient.Requests.Project;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class RegisterPipelineFunction
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly ILoggingService _loggingService;
    private readonly IPipelineRegistrator _registrator;
    private readonly Core.InputValidation.Services.IValidateInputService _validateInputService;

    public RegisterPipelineFunction(
        IAzdoRestClient azdoClient,
        IPipelineRegistrator registrator,
        Core.InputValidation.Services.IValidateInputService validateInputService,
        IAuthorizationService authorizationService,
        ICompliancyReportService compliancyReportService,
        ILoggingService loggingService)
    {
        _azdoClient = azdoClient;
        _registrator = registrator;
        _validateInputService = validateInputService;
        _authorizationService = authorizationService;
        _compliancyReportService = compliancyReportService;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(RegisterPipelineFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "register/{organization}/{projectId}/{pipelineId}/{pipelineType}")]
        HttpRequestMessage request, string organization, string projectId,
        string pipelineId, string pipelineType)
    {
        RegistrationRequest? input = null;

        try
        {
            _validateInputService.Validate(request, organization, projectId, pipelineId);
            _validateInputService.ValidateItemType(pipelineType,
                new[] { ItemTypes.YamlReleasePipeline, ItemTypes.ClassicReleasePipeline });

            if (!await _authorizationService.HasEditPermissionsAsync(
                    request, organization, projectId, pipelineId, pipelineType))
            {
                return new UnauthorizedObjectResult(ErrorMessages.Unauthorized);
            }

            input = await GetInputAsync(request);
            if (input == null)
            {
                return new BadRequestResult();
            }

            var interactiveUser = await _authorizationService.GetInteractiveUserAsync(request, organization);
            var result = input.CiIdentifier == null
                ? await _registrator.RegisterNonProdPipelineAsync(organization, projectId, pipelineId, pipelineType,
                    input.Environment)
                : await _registrator.RegisterProdPipelineAsync(organization, projectId, pipelineId, pipelineType, interactiveUser.MailAddress,
                    input);

            var project = await _azdoClient.GetAsync(Project.ProjectById(projectId), organization);

            if (result is OkResult)
            {
                await _compliancyReportService.UpdateRegistrationAsync(
                    organization, project.Name, pipelineId, pipelineType, input.CiIdentifier);
            }

            return result;
        }
        catch (Exception ex) when (ex is ItemNotFoundException or ArgumentException)
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

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
                (nameof(RegisterPipelineFunction), organization, projectId, request.RequestUri?.AbsoluteUri);

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, e, pipelineId, input?.Environment, input?.CiIdentifier);
            return exceptionBaseMetaInformation;
        }
    }

    private static async Task<RegistrationRequest?> GetInputAsync(HttpRequestMessage request)
    {
        if (request.Content == null)
        {
            return null;
        }

        var content = await request.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<RegistrationRequest>(content);
    }
}