using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;
using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Pipeline;

/// <inheritdoc/>
public class GetPipelineApprovalsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Approval>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly string _approvalIds;

    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/approvals";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.1-preview.1" },
        { "approvalIds", _approvalIds },
        { "&$expand", "steps" }
    };

    /// <summary>
    /// Gets approvals of a pipeline, but uses the approvalIds filter for it, 
    /// so make sure to fetch them first through GetBuildTimelineRequest. This
    /// request will give you all approvalIds when you filter it by the type
    /// Checkpoint.Approval and use the identifier property.
    /// </summary>
    /// <remarks>PreviewRun if true, don't actually create a new run. Instead, return the final YAML document after parsing templates.</remarks>
    public GetPipelineApprovalsRequest(string organization, Guid projectId, Guid[] approvalIds, IDevHttpClientCallHandler httpClientCallHandler
        )
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _approvalIds = string.Join(",", approvalIds);
    }
}