#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class DeletePipelineRegistrationFunction
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;
    private readonly ILoggingService _loggingService;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly IPipelineRegistrator _pipelineRegistrator;
    private readonly IValidateInputService _validateInputService;

    public DeletePipelineRegistrationFunction(
        IPipelineRegistrator pipelineRegistrator,
        IAuthorizationService authorizationService,
        IValidateInputService validateInputService,
        IAzdoRestClient azdoClient,
        ICompliancyReportService compliancyReportService,
        ICheckAuthorizationProcess checkAuthorizationProcess,
        ILoggingService loggingService)
    {
        _pipelineRegistrator = pipelineRegistrator;
        _authorizationService = authorizationService;
        _validateInputService = validateInputService;
        _azdoClient = azdoClient;
        _compliancyReportService = compliancyReportService;
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(DeletePipelineRegistrationFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "delete",
            Route = "deleteregistration/{organization}/{projectId}/{pipelineId}/{pipelineType}")]
        HttpRequestMessage request, string? organization, Guid projectId,
        string pipelineId, string pipelineType, CancellationToken cancellationToken = default)
    {
        try
        {
            _validateInputService.Validate(request, organization, projectId.ToString());

            var deleteRequest = await request.DeserializeContentAsync<DeleteRegistrationRequest>();
            if (deleteRequest == null)
            {
                return new BadRequestResult();
            }

            // validate request
            var validationResult = Validate(deleteRequest);
            if (validationResult.IsNotNullOrWhiteSpace())
            {
                throw new ValidationException(validationResult);
            }

            var authorizationRequest = new AuthorizationRequest(projectId, organization);

            if (!await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, request.Headers.Authorization, cancellationToken))
            {
                return new UnauthorizedResult();
            }

            var interactiveUser = await _authorizationService.GetInteractiveUserAsync(request, organization);
            var result = await _pipelineRegistrator.DeleteProdPipelineRegistrationAsync(organization,
                projectId.ToString(), pipelineId, pipelineType, interactiveUser.MailAddress, deleteRequest);

            if (result is OkObjectResult)
            {
                var project = await _azdoClient.GetAsync(Project.ProjectById(projectId.ToString()), organization);

                await _compliancyReportService.UnRegisteredPipelineAsync(organization, project.Name, pipelineId,
                    pipelineType, cancellationToken);
            }

            return result;
        }
        catch (Exception ex) when (ex is ArgumentException or ValidationException)
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
                (request, nameof(DeletePipelineRegistrationFunction), projectId.ToString())
            {
                Organization = organization,
            };

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);
            return exceptionBaseMetaInformation;
        }
    }

    private static string Validate(DeleteRegistrationRequest registrationRequest)
    {
        var sb = new StringBuilder();

        if (string.IsNullOrWhiteSpace(registrationRequest.Environment))
        {
            sb.AppendLine(string.Format(Constants.IsRequiredText, nameof(registrationRequest.Environment)));
        }

        if (string.IsNullOrWhiteSpace(registrationRequest.CiIdentifier))
        {
            sb.AppendLine(string.Format(Constants.IsRequiredText, nameof(registrationRequest.CiIdentifier)));
        }

        return sb.ToString();
    }
}