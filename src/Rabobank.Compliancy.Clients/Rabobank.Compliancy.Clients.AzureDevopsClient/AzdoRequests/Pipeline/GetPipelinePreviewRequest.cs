using Microsoft.Azure.Pipelines.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Pipeline;

/// <inheritdoc/>
public class PostPipelinePreviewRequest : HttpPostRequest<IDevHttpClientCallHandler, PreviewRun, object>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _pipelineId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/{_pipelineId}/preview";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PostPipelinePreviewRequest"/> class.
    /// </summary>
    /// <remarks>PreviewRun if true, don't actually create a new run. Instead, return the final YAML document after parsing templates.</remarks>
    public PostPipelinePreviewRequest(string organization, Guid projectId, int pipelineId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(new { PreviewRun = true }, httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _pipelineId = pipelineId;
    }
}