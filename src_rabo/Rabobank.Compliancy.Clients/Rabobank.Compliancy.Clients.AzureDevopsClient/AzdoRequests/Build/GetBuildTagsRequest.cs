using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Gets the tags for a build.
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/build/tags/get-build-tags?view=azure-devops-rest-7.0
/// </summary>
public class GetBuildTagsRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<string>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _buildId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/builds/{_buildId}/tags";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public GetBuildTagsRequest(string organization, Guid projectId, int buildId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildId = buildId;
    }
}