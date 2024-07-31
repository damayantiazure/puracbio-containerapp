#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using ErrorMessages = Rabobank.Compliancy.Functions.ComplianceScanner.Online.Exceptions.ErrorMessages;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class ProjectRescanFunction
{
    private const int _parallelCiScans = 10;

    private readonly IAzdoRestClient _azdoClient;
    private readonly IScanProjectService _scanProjectService;
    private readonly ILoggingService _loggingService;
    private readonly IValidateInputService _validateInputService;

    public ProjectRescanFunction(
        IAzdoRestClient azdoClient,
        IValidateInputService validateInputService,
        IScanProjectService scanProjectService,
        ILoggingService loggingService)
    {
        _azdoClient = azdoClient;
        _validateInputService = validateInputService;
        _scanProjectService = scanProjectService;
        _loggingService = loggingService;
    }

    [FunctionName(nameof(ProjectRescanFunction))]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "scan/{organization}/{projectId}")]
        HttpRequestMessage request, string organization, string projectId)
    {
        try
        {
            var scanDate = DateTime.UtcNow;

            _validateInputService.Validate(request, organization, projectId);

            var project = await _azdoClient.GetAsync(Project.ProjectById(projectId), organization);

            var complianceReport = await _scanProjectService.ScanProjectAsync(
                organization, project, scanDate, _parallelCiScans);

            await CheckCiReportsForErrorsAsync(request, organization, projectId, complianceReport);

            return new OkResult();
        }
        catch (Exception ex) when (ex is ScanException or ArgumentException)
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
                (nameof(ProjectRescanFunction), organization, projectId, request.RequestUri?.AbsoluteUri);

            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex);
            return exceptionBaseMetaInformation;
        }
    }

    private async Task CheckCiReportsForErrorsAsync(HttpRequestMessage request, string organization,
        string projectId, CompliancyReport complianceReport)
    {
        if (complianceReport.RegisteredConfigurationItems == null ||
            !complianceReport.RegisteredConfigurationItems.Any(x => x.IsScanFailed))
        {
            return;
        }

        var failedCis = complianceReport.RegisteredConfigurationItems
            .Where(ciReport => ciReport.IsScanFailed)
            .ToDictionary(ciReport => ciReport.Id, ciReport => ciReport.ScanException);

        var exceptionBaseMetaInformation = new ExceptionBaseMetaInformation
            (nameof(ProjectRescanFunction), organization, projectId, request.RequestUri?.AbsoluteUri);

        await Task.WhenAll(failedCis
            .Select(async keyValuePair =>
                await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, keyValuePair.Key, keyValuePair.Value!)));

        throw new ScanException(ErrorMessages.CiScanFailures(
            failedCis.First().Key, failedCis.First().Value?.ExceptionMessage));
    }
}