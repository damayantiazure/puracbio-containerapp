#nullable enable

using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Activities;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient.Helpers;

namespace Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;

public class ValidateApproversOrchestrator
{
    [FunctionName(nameof(ValidateApproversOrchestrator))]
    public Task RunAsync([OrchestrationTrigger] IDurableOrchestrationContext context)
    {
        var azdoData = context.GetInput<ValidateApproversAzdoData>();
        if (azdoData == null || !azdoData.IsValid)
        {
            throw new ArgumentException(nameof(azdoData));
        }

        return RunAsyncInternal(context, azdoData);
    }

    private static async Task RunAsyncInternal(IDurableOrchestrationContext context, ValidateApproversAzdoData azdoData)
    {
        try
        {
            await context.CallActivityWithRetryAsync(nameof(SendTaskStartedActivity), RetryHelper.ActivityRetryOptions,
                azdoData);

            var result = default(ValidateApproversResult);
            var organization = azdoData.SmoketestOrganization ?? azdoData.Organization;

            if (azdoData.RunId != null)
            {
                result = await context.CallActivityWithRetryAsync<ValidateApproversResult>(
                    nameof(ValidateYamlApproversActivity),
                    RetryHelper.ActivityRetryOptions, (azdoData.ProjectId, azdoData.RunId, organization));
            }

            if (azdoData.Release != null)
            {
                result = await context.CallActivityWithRetryAsync<ValidateApproversResult>(
                    nameof(ValidateClassicApproversActivity),
                    RetryHelper.ActivityRetryOptions, (azdoData.ProjectId, azdoData.Release, organization));
            }

            await context.CallActivityWithRetryAsync(nameof(AppendToTaskLogActivity), RetryHelper.ActivityRetryOptions,
                (azdoData, result?.Message));

            var hasApproval = result?.DeterminedApprovalType == ApprovalType.PipelineApproval ||
                              result?.DeterminedApprovalType == ApprovalType.PullRequestApproval;

            await context.CallActivityWithRetryAsync(nameof(SendTaskCompletedActivity),
                RetryHelper.ActivityRetryOptions, (azdoData, hasApproval));
        }
        catch (Exception ex) when (ex.InnerException is OrchestrationSessionNotFoundException)
        {
            // In case of OrchestrationSessionNotFoundException retry is useless, we only log the exception
            await HandleException(context, azdoData, ex);
        }
        catch (Exception ex)
        {
            try
            {
                // Try to conclude the callback cycle with a meaningful piece of information
                const string message = "Something unexpected happened while validating 4-eyes approval.";
                await context.CallActivityWithRetryAsync(nameof(AppendToTaskLogActivity),
                    RetryHelper.ActivityRetryOptions, (azdoData,
                        message));
                await context.CallActivityWithRetryAsync(nameof(SendTaskCompletedActivity),
                    RetryHelper.ActivityRetryOptions, (azdoData, false));
            }
            catch (Exception ex2)
            {
                // Always send a callback in case of an error.
                await context.CallActivityWithRetryAsync(nameof(SendTaskCompletedActivity),
                    RetryHelper.ActivityRetryOptions, (azdoData, false));
                await HandleException(context, azdoData, ex2);
            }
            finally
            {
                await HandleException(context, azdoData, ex);
            }
        }
    }

    private static Task HandleException(
        IDurableOrchestrationContext context, ValidateApproversAzdoData azdoData, Exception ex)
    {
        var exceptionReport = new ExceptionReport(ex)
        {
            FunctionName = nameof(ValidateApproversOrchestrator),
            ProjectId = azdoData.ProjectId,
            RunId = azdoData.RunId,
            ReleaseId = azdoData.Release?.Id.ToString(),
            StageId = azdoData.StageId
        };

        return context.CallActivityWithRetryAsync(nameof(UploadExceptionToLogAnalyticsActivity),
            RetryHelper.ActivityRetryOptions, exceptionReport);
    }
}