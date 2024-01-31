using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Gets details for a build
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/build/timeline/get?view=azure-devops-rest-7.0
/// </summary>
public class GetBuildTimelineRequest : HttpGetRequest<IDevHttpClientCallHandler, Timeline>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _buildId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/builds/{_buildId}/timeline";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetBuildTimelineRequest(string organization, Guid projectId, int buildId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildId = buildId;
    }
}