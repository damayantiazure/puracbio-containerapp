using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

internal class GetReleaseTagsRequest : HttpGetRequest<IVsrmHttpClientCallHandler, ResponseCollection<string>>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _releaseId;

    public GetReleaseTagsRequest(string organization, Guid projectId, int releaseId,
        IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _releaseId = releaseId;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/releases/{_releaseId}/tags";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}