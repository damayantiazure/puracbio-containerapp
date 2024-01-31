using Microsoft.VisualStudio.Services.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.Helpers;

public static class EnsureOperationHelper
{
    public static async Task EnsureOperationCompletedAsync(this IOperationRepository operationRepository, string organization, Guid? operationId, CancellationToken cancellationToken = default)
    {
        if (operationId == null || operationId == Guid.Empty)
        {
            throw new InvalidOperationException("Operation ID is not valid");
        }

        await RetryUntilDoneOrTimeout(() => operationRepository.OperationIsInProgressAsync(organization, operationId.GetValueOrDefault(), cancellationToken), TimeSpan.FromMinutes(2), cancellationToken);

        var operation = await operationRepository.GetOperationReferenceByIdAsync(organization, operationId.GetValueOrDefault(), cancellationToken);

        if (operation == null || operation.Status != OperationStatus.Succeeded)
        {
            throw new InvalidOperationException($"Operation id {operationId} did not succeed or timed out.");
        }
    }

    private static async Task RetryUntilDoneOrTimeout(Func<Task<bool>> task, TimeSpan timeSpan, CancellationToken cancellationToken = default)
    {
        bool inProgress = true;
        int elapsed = 0;
        while (inProgress && (elapsed < timeSpan.TotalMilliseconds))
        {
            if (cancellationToken.IsCancellationRequested)
            {
                throw new OperationCanceledException();
            }
            Thread.Sleep(1000);
            elapsed += 1000;
            inProgress = await task();
        }
    }
}