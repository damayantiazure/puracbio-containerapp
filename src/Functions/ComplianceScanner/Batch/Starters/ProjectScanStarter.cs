using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Starters;

public class ProjectScanStarter
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly string[] _organizations;

    public ProjectScanStarter(
        IAzdoRestClient azdoClient,
        string[] organizations)
    {
        _azdoClient = azdoClient;
        _organizations = organizations;
    }

    [FunctionName(nameof(ProjectScanStarter))]
    public Task RunAsync(
        [TimerTrigger("0 0 19 * * *", RunOnStartup = false)] TimerInfo timerInfo,
        [DurableClient] IDurableOrchestrationClient durableClient)
    {
        if (durableClient == null)
        {
            throw new ArgumentNullException(nameof(durableClient));
        }

        return RunInternalAsync(durableClient);
    }

    private async Task RunInternalAsync(IDurableOrchestrationClient durableClient)
    {
        var scanDate = DateTime.UtcNow;

        await Task.WhenAll(_organizations.Select(async o =>
            await ScanOrganization(o, scanDate, durableClient)));
    }

    private async Task ScanOrganization(string organization, DateTime scanDate, 
        IDurableOrchestrationClient durableClient)
    {
        var projects = (await _azdoClient.GetAsync(Project.Projects(), organization))
            .OrderBy(p => p.Name)
            .ToList();
        var input = new Tuple<string, List<Response.Project>, DateTime>
            (organization, projects, scanDate);
        await durableClient.StartNewAsync(nameof(ProjectScanSupervisor), input);
    }
}