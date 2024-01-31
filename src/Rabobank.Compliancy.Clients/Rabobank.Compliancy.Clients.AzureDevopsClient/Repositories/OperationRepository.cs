using Microsoft.VisualStudio.Services.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Operations;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class OperationsRepository : IOperationRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public OperationsRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<Operation?> GetOperationReferenceByIdAsync(string organization, Guid id, CancellationToken cancellationToken = default)
    {
        var request = new GetOperationRequest(organization, id, _httpClientCallHandler);
        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<bool> OperationIsInProgressAsync(string organization, Guid id, CancellationToken cancellationToken = default)
    {
        var request = new GetOperationRequest(organization, id, _httpClientCallHandler);
        var operation = await request.ExecuteAsync(cancellationToken: cancellationToken);

        return operation != null && IsInProgress(operation.Status);
    }

    private static bool IsInProgress(OperationStatus operationStatus) =>
        (operationStatus == OperationStatus.NotSet || operationStatus == OperationStatus.Queued || operationStatus == OperationStatus.InProgress);
}