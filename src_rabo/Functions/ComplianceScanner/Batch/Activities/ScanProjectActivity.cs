#nullable enable

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Threading.Tasks;
using Flurl.Http;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;

public class ScanProjectActivity
{
    private const int _parallelCiScans = 5;

    private readonly IScanProjectService _scanProjectService;

    public ScanProjectActivity(IScanProjectService scanProjectService) =>
        _scanProjectService = scanProjectService;

    [FunctionName(nameof(ScanProjectActivity))]
    public async Task<CompliancyReport> RunAsync([ActivityTrigger]
        (string organization, Response.Project project, DateTime scanDate) input)
    {
        try
        {
            var (organization, project, scanDate) = input;
            return await _scanProjectService.ScanProjectAsync(organization, project, scanDate, _parallelCiScans);
        }
        catch (FlurlHttpException ex)
        {
            throw await ex.MakeDurableFunctionCompatible();
        }
    }
}