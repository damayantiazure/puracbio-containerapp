using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Activities;
using Rabobank.Compliancy.Infra.AzdoClient.Helpers;
using System;
using System.Linq;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch.Orchestrators;

public class ProjectScanOrchestrator
{
    [FunctionName(nameof(ProjectScanOrchestrator))]
    public Task RunAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return RunInternalAsync(context);
    }

    private static async Task RunInternalAsync(IDurableOrchestrationContext context)
    {
        var (organization, project, scanDate) =
            context.GetInput<(string, Infra.AzdoClient.Response.Project, DateTime)>();

        try
        {
            var complianceReport = await context.CallActivityWithRetryAsync<CompliancyReport>(
                nameof(ScanProjectActivity), RetryHelper.ActivityRetryOptions,
                (organization, project, scanDate));

            await context.CallActivityWithRetryAsync(nameof(UploadCompliancyToLogAnalyticsActivity),
                RetryHelper.ActivityRetryOptions, (organization, project.Id, complianceReport));

            if (complianceReport.RegisteredConfigurationItems == null)
            {
                throw new InvalidOperationException(
                    $"Cannot be null: {nameof(complianceReport.RegisteredConfigurationItems)}");
            }

            if (complianceReport.RegisteredConfigurationItems.Any(x => x.IsScanFailed))
            {
                var failedCis = complianceReport.RegisteredConfigurationItems
                    .Where(x => x.IsScanFailed)
                    .ToDictionary(x => x.Id, x => x.ScanException);

                await Task.WhenAll(failedCis
                    .Select(async c =>
                    {
                        var message = $"Could not scan {organization}/{project.Id} for ci {c.Key}." +
                                      $"Exception: {c.Value?.ExceptionMessage}. " + $"Innerexception: {c.Value?.InnerExceptionMessage}";

                        var exceptionReport = new ExceptionReport
                        {
                            FunctionName = nameof(ProjectScanOrchestrator),
                            Organization = organization,
                            ProjectId = project.Id,
                            ExceptionMessage = message
                        };

                        await context.CallActivityWithRetryAsync(nameof(UploadExceptionReportToLogAnalyticsActivity),
                            RetryHelper.ActivityRetryOptions, exceptionReport);
                    }));
            }
        }
        catch (Exception exception)
        {
            var exceptionReport = new ExceptionReport(exception)
            {
                FunctionName = nameof(ProjectScanOrchestrator),
                Organization = organization,
                ProjectId = project.Id
            };

            await context.CallActivityWithRetryAsync(nameof(UploadExceptionReportToLogAnalyticsActivity),
                RetryHelper.ActivityRetryOptions, exceptionReport);
        }
    }
}