using Microsoft.Azure.Pipelines.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Pipeline;

/// <inheritdoc/>
public class GetPipelineRunRequest : HttpGetRequest<IDevHttpClientCallHandler, Run>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _pipelineId;
    private readonly int _runId;
    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/{_pipelineId}/runs/{_runId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    /// <summary>
    /// Gets a pipelinerun by ID.
    /// </summary>
    /// <remarks>PreviewRun if true, don't actually create a new run. Instead, return the final YAML document after parsing templates.</remarks>
    public GetPipelineRunRequest(string organization, Guid projectId, int pipelineId, int runId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _pipelineId = pipelineId;
        _runId = runId;
    }
}