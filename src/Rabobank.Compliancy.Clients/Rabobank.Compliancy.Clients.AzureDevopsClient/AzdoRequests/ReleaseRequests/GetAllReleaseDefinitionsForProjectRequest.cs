using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

/// <summary>
/// Used to get ReleaseDefinitions by project from the URL "{_organization}/{_projectId}/_apis/release/definitions/".
/// </summary>
public class GetAllReleaseDefinitionsForProjectRequest : HttpGetRequest<IVsrmHttpClientCallHandler, ResponseCollection<Microsoft.VisualStudio.Services.ReleaseManagement.WebApi.ReleaseDefinition>>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/definitions/";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "$expand", "Artifacts,Environments" },
        {"api-version", "7.0"}
    };

    public GetAllReleaseDefinitionsForProjectRequest(string organization, Guid projectId, IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}