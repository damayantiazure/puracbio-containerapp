using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Helpers;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;
using System;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;

public class ProjectScanSupervisor
{
    [FunctionName(nameof(ProjectScanSupervisor))]
    public async Task RunAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var (organization, projects, scanDate) = 
            context.GetInput<(string, List<Project>, DateTime)>();

        foreach (var project in projects)
        {
            await context.CallSubOrchestratorAsync(nameof(ProjectScanOrchestrator),
                OrchestrationHelper.CreateProjectScanOrchestrationId(context.InstanceId, project.Id),
                (organization, project, scanDate));
        }
    }
}