#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using Rabobank.Compliancy.Core.InputValidation.Services;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class CiRescanFunction
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly IPipelineRegistrationRepository _pipelineRegistrationRepository;
    private readonly IScanCiService _scanCiService;
    private readonly IValidateInputService _validateInputService;
    private readonly ILoggingService _loggingService;

    public CiRescanFunction(
        IAzdoRestClient azdoClient,
        IValidateInputService validateInputService,
        IScanCiService scanCiService,
        ICompliancyReportService compliancyReportService,
        IPipelineRegistrationRepository pipelineRegistrationRepository,
        ILoggingService loggingService)
    {
        _azdoClient = azdoClient;
        _validateInputService = validateInputService;
        _scanCiService = scanCiService;
        _compliancyReportService = compliancyReportService;
        _pipelineRegistrationRepository = pipelineRegistrationRepository;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(CiRescanFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "scan/{organization}/{projectId}/{ciIdentifier}")]
        HttpRequestMessage request, string organization, string projectId, string ciIdentifier)
    {
        try
        {
            var scanDate = DateTime.UtcNow;

            _validateInputService.Validate(organization, projectId, ciIdentifier, request);
            var pipelineRegistrations = await _pipelineRegistrationRepository.GetAsync(organization, projectId);

            var project = await _azdoClient.GetAsync(Project.ProjectById(projectId), organization);

            var newCiReport = await _scanCiService.ScanCiAsync(
                organization, project, ciIdentifier, scanDate, pipelineRegistrations);

            await _compliancyReportService.UpdateCiReportAsync(organization, Guid.Parse(project.Id), project.Name,
                newCiReport, scanDate);

            return new OkResult();
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
                (request, nameof(CiRescanFunction), projectId)
            {
                Organization = organization
            };

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex, ciIdentifier);
            return exceptionBaseMetaInformation;
        }
    }
}