
#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Interfaces;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Domain.Extensions;
using Rabobank.Compliancy.Domain.RuleProfiles;
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
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;
using Rabobank.Compliancy.Core.InputValidation.Services;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class UpdatePipelineRegistrationFunction
{
    private readonly IAuthorizationService _authorizationService;
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICheckAuthorizationProcess _checkAuthorizationProcess;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly ILoggingService _loggingService;
    private readonly IPipelineRegistrator _registrator;
    private readonly IValidateInputService _validateInputService;

    public UpdatePipelineRegistrationFunction(
        IAzdoRestClient azdoClient,
        IPipelineRegistrator registrator,
        IValidateInputService validateInputService,
        IAuthorizationService authorizationService,
        ICheckAuthorizationProcess checkAuthorizationProcess,
        ICompliancyReportService compliancyReportService,
        ILoggingService loggingService)
    {
        _azdoClient = azdoClient;
        _registrator = registrator;
        _validateInputService = validateInputService;
        _authorizationService = authorizationService;
        _checkAuthorizationProcess = checkAuthorizationProcess;
        _compliancyReportService = compliancyReportService;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(UpdatePipelineRegistrationFunction))]
    public async Task<IActionResult?> RunAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, Route = "updateregistration/{organization}/{projectId}/{pipelineId}/{pipelineType}")]
        HttpRequestMessage request, string organization, Guid projectId,
        string pipelineId, string pipelineType)
    {
        try
        {
            _validateInputService.Validate(request, organization, projectId.ToString(), pipelineId);
            _validateInputService.ValidateItemType(pipelineType,
                new[] { ItemTypes.YamlReleasePipeline, ItemTypes.ClassicReleasePipeline });

            pipelineType =
                pipelineType.Equals(ItemTypes.YamlReleasePipeline, StringComparison.InvariantCultureIgnoreCase)
                    ? ItemTypes.YamlReleasePipeline
                    : ItemTypes.ClassicReleasePipeline;

            var updateRequest = await request.DeserializeContentAsync<UpdateRequest>();
            var validateMessage = Validate(updateRequest);

            if (!string.IsNullOrWhiteSpace(validateMessage))
            {
                throw new ValidationException(validateMessage);
            }

            var authorizationRequest = new AuthorizationRequest(projectId, organization);

            if (!await _checkAuthorizationProcess.IsAuthorized(authorizationRequest, request.Headers.Authorization))
            {
                return new UnauthorizedResult();
            }

            IActionResult? result = null;

            // Update the registrations when the ci identifier is not empty otherwise update non prod registrations
            if (updateRequest != null && !string.IsNullOrWhiteSpace(updateRequest.CiIdentifier))
            {
                var interactiveUser = await _authorizationService.GetInteractiveUserAsync(request, organization);
                result = await _registrator.UpdateProdPipelineRegistrationAsync(organization, projectId.ToString(),
                    pipelineId, pipelineType, interactiveUser.MailAddress, updateRequest);

                if (result is OkObjectResult)
                {
                    var project = await _azdoClient.GetAsync(Project.ProjectById(projectId.ToString()), organization);
                    await _compliancyReportService.UpdateRegistrationAsync(
                        organization, project.Name, pipelineId, pipelineType, updateRequest.CiIdentifier);
                }
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

        async Task<ExceptionBaseMetaInformation> LogExceptionAsync(Exception e)
        {
            var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
            (nameof(UpdatePipelineRegistrationFunction), organization, projectId.ToString(),
                request.RequestUri?.AbsoluteUri);

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, e, exceptionBaseMetaInformation,
                pipelineId, pipelineType);
            return exceptionBaseMetaInformation;
        }
    }

    private static string Validate(UpdateRequest updateRequest)
    {
        var sb = new StringBuilder();

        if (string.IsNullOrWhiteSpace(updateRequest.Environment))
        {
            sb.AppendLine(string.Format(Constants.IsRequiredText, nameof(updateRequest.Environment)));
        }

        if (string.IsNullOrWhiteSpace(updateRequest.NewValue))
        {
            sb.AppendLine(string.Format(Constants.IsRequiredText, nameof(updateRequest.NewValue)));
        }

        if (string.IsNullOrWhiteSpace(updateRequest.FieldToUpdate))
        {
            sb.AppendLine(string.Format(Constants.IsRequiredText, nameof(updateRequest.FieldToUpdate)));
        }

        var fieldToUpdate = EnumHelper.ParseEnumOrNull<FieldToUpdate>(updateRequest.FieldToUpdate);
        if (fieldToUpdate == null)
        {
            sb.AppendLine(string.Format(Constants.IsInvalidText, nameof(updateRequest.FieldToUpdate)));
        }

        if (!string.IsNullOrWhiteSpace(updateRequest.Profile)
            && EnumHelper.ParseEnumOrNull<Profiles>(updateRequest.Profile) == null)
        {
            sb.AppendLine(string.Format(Constants.IsInvalidText, nameof(updateRequest.Profile)));
        }

        if (fieldToUpdate == FieldToUpdate.Profile
            && EnumHelper.ParseEnumOrNull<Profiles>(updateRequest.NewValue) == null)
        {
            sb.AppendLine(string.Format(Constants.IsInvalidText, nameof(updateRequest.NewValue)));
        }

        return sb.ToString();
    }
}