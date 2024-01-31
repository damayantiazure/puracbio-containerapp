using Microsoft.Azure.Pipelines.WebApi;
using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Pipeline;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;

/// <inheritdoc/>
public class PipelineRepository : IPipelineRepository
{
    private readonly IDevHttpClientCallHandler _httpClientCallHandler;

    public PipelineRepository(IDevHttpClientCallHandler httpClientCallHandler)
    {
        _httpClientCallHandler = httpClientCallHandler;
    }

    /// <inheritdoc/>
    public async Task<string?> GetPipelineYamlFromPreviewRunAsync(string organization, Guid projectId, int pipelineId, CancellationToken cancellationToken = default)
    {
        var request = new PostPipelinePreviewRequest(organization, projectId, pipelineId, _httpClientCallHandler);

        return (await request.ExecuteAsync(cancellationToken: cancellationToken))?.FinalYaml;
    }

    /// <inheritdoc/>
    public async Task<Run?> GetPipelineRunAsync(string organization, Guid projectId, int pipelineId, int runId, CancellationToken cancellationToken = default)
    {
        var request = new GetPipelineRunRequest(organization, projectId, pipelineId, runId, _httpClientCallHandler);

        return await request.ExecuteAsync(cancellationToken: cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<IEnumerable<string>?> GetBuildApproverByTimelineAsync(string organization, Guid projectId, Timeline? timeline, CancellationToken cancellationToken = default)
    {
        if (timeline == null || timeline.Records == null || !timeline.Records.Any())
        {
            return null;
        }

        var approvals = await
            new GetPipelineApprovalsRequest(organization, projectId,
                timeline.Records
                .Where(record => record.RecordType == "Checkpoint.Approval")
                .Select(record => new Guid(record.Identifier)).ToArray(),
                _httpClientCallHandler)
            .ExecuteAsync(cancellationToken: cancellationToken);

        return approvals?.Value!
            .Where(approval => approval.Status == "approved")
            .SelectMany(approval => approval.Steps)
            .Where(step => step.Status == "approved")
            .Select(step => step.AssignedApprover.UniqueName);
    }
}