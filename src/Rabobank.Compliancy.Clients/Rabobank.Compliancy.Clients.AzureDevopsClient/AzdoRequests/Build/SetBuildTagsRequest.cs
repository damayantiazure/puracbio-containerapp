using Microsoft.TeamFoundation.Build.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Build;

/// <summary>
/// Adds or removes a tag to/from a build.
/// https://learn.microsoft.com/en-us/rest/api/azure/devops/build/tags/update-build-tags?view=azure-devops-rest-7.0
/// </summary>
public class SetBuildTagsRequest : HttpPatchRequest<IDevHttpClientCallHandler, ResponseCollection<string>, UpdateTagParameters>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _buildId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/build/builds/{_buildId}/tags/";

    protected override Dictionary<string, string> QueryStringParameters { get; } = new()
    {
        { "api-version", "7.0" }
    };

    public SetBuildTagsRequest(string organization, Guid projectId, int buildId, UpdateTagParameters updateBuildTags,
        IDevHttpClientCallHandler callHandler) : base(updateBuildTags, callHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _buildId = buildId;
    }
}