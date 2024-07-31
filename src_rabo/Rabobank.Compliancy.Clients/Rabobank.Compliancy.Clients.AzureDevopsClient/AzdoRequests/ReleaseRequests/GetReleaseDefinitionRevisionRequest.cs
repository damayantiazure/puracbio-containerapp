using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

/// <summary>
/// Used to get a ReleaseDefinitionRevision by ID from the URL "{_organization}/{_projectId}/_apis/release/definitions/{_releaseDefinitionId}/revisions/{_revisionId}".
/// </summary>
public class GetReleaseDefinitionRevisionRequest : HttpGetRequest<IVsrmHttpClientCallHandler, string>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _releaseDefinitionId;
    private readonly int _revisionId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/definitions/{_releaseDefinitionId}/revisions/{_revisionId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetReleaseDefinitionRevisionRequest(string organization, Guid projectId, int releaseDefinitionId, int revisionId, IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _releaseDefinitionId = releaseDefinitionId;
        _revisionId = revisionId;
    }
}