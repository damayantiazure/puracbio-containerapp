using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

/// <summary>
/// Used to get ReleaseDefinitions by ID from the URL "{_organization}/{_projectId}/_apis/release/definitions/{_releaseDefinitionId}".
/// </summary>
public class GetReleaseDefinitionRequest : HttpGetRequest<IVsrmHttpClientCallHandler, Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.ReleaseDefinition>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly int _releaseDefinitionId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/definitions/{_releaseDefinitionId}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"api-version", "7.0"}
    };

    public GetReleaseDefinitionRequest(string organization, Guid projectId, int releaseDefinitionId, IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _releaseDefinitionId = releaseDefinitionId;
    }
}