using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

public class CheckConfigurationRepository : ICheckConfigurationRepository
{
    /* Different resource types */
    private const string environment = nameof(environment);

    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public CheckConfigurationRepository(IDevHttpClientCallHandler httpClientCallHandler) =>
        _httpClientCallHandler = httpClientCallHandler;

    /// <inheritdoc />
    public async Task<IEnumerable<CheckConfiguration>?> GetCheckConfigurationsForEnvironmentAsync(
        string organization, Guid projectId, int environmentId, CancellationToken cancellationToken = default)
    {
        var request = new GetEnvironmentCheckRequest(organization, projectId, environment, environmentId.ToString(),
            _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.Value;
    }

    /// <inheritdoc />
    public async Task<CheckConfiguration?> CreateCheckForEnvironmentAsync(
        string organization, Guid projectId, EnvironmentCheckBodyContent content,
        CancellationToken cancellationToken = default)
    {
        var request = new CreateEnvironmentCheckRequest(organization, projectId, content, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc />
    public async Task DeleteCheckForEnvironmentAsync(
        string organization, Guid projectId, string id,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteEnvironmentCheckRequest(organization, projectId, id, _httpClientCallHandler);

        await request.ExecuteAsync(cancellationToken: cancellationToken);
    }
}