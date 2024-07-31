#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Rabobank.Compliancy.Application.Requests;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class NonProdPipelineRescanFunction : BaseFunction
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly ICompliancyReportService _compliancyReportService;
    private readonly IPipelineRegistrationRepository _pipelineRegistrationRepository;
    private readonly IScanCiService _scanCiService;
    private readonly ILoggingService _loggingService;

    public NonProdPipelineRescanFunction(
        IAzdoRestClient azdoClient, ILoggingService loggingService, IScanCiService scanCiService,
        ICompliancyReportService compliancyReportService, IPipelineRegistrationRepository pipelineRegistrationRepository,
        IHttpContextAccessor httpContextAccessor, ISecurityContext securityContext)
        : base(httpContextAccessor, loggingService, securityContext)
    {
        _azdoClient = azdoClient;
        _loggingService = loggingService;
        _scanCiService = scanCiService;
        _compliancyReportService = compliancyReportService;
        _pipelineRegistrationRepository = pipelineRegistrationRepository;
    }

    /// <summary>
    ///     Rescans a non-prod pipeline
    /// </summary>
    /// <param name="rescanPipelineRequest"></param>
    /// <param name="httpRequest"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    [FunctionName(nameof(NonProdPipelineRescanFunction))]
    public Task<IActionResult> ScanNonProdPipeline(
        [HttpTrigger(AuthorizationLevel.Anonymous,
            Route = "scanpipeline/{organization}/{projectId}/{pipelineId}")]
        RescanPipelineRequest rescanPipelineRequest, HttpRequest httpRequest,
        CancellationToken cancellationToken = default) =>
        HandlePipelineRescanProcess(rescanPipelineRequest, httpRequest, nameof(NonProdPipelineRescanFunction));

    private async Task<IActionResult> HandlePipelineRescanProcess(RescanPipelineRequest rescanPipelineRequest,
        HttpRequest httpRequest, string functionName)
    {
        try
        {
            var pipelineRegistrations =
                await _pipelineRegistrationRepository.GetAsync(rescanPipelineRequest.Organization,
                    rescanPipelineRequest.ProjectId.ToString());

            var project = await _azdoClient.GetAsync(Project.ProjectById(rescanPipelineRequest.ProjectId.ToString()),
                rescanPipelineRequest.Organization);

            var nonProdReport = await _scanCiService.ScanNonProdPipelineAsync(rescanPipelineRequest.Organization,
                project, DateTime.UtcNow, rescanPipelineRequest.PipelineId.ToString(), pipelineRegistrations);

            await _compliancyReportService.UpdateNonProdPipelineReportAsync(rescanPipelineRequest.Organization,
                project.Name, nonProdReport);

            return new OkResult();
        }
        catch (Exception ex)
        {
            var exceptionBaseMetaInformation =
                httpRequest.ToExceptionBaseMetaInformation(rescanPipelineRequest.Organization,
                    rescanPipelineRequest.ProjectId, functionName);
            await _loggingService.LogExceptionAsync(LogDestinations.ComplianceScannerOnlineErrorLog, exceptionBaseMetaInformation, ex, rescanPipelineRequest.PipelineId.ToString());
            throw;
        }
    }
}