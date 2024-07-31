using Microsoft.VisualStudio.Services.ReleaseManagement.WebApi;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.ReleaseRequests;

public class GetReleaseSettingsRequest : HttpGetRequest<IVsrmHttpClientCallHandler, ReleaseSettings>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    public GetReleaseSettingsRequest(string organization, Guid projectId,
        IVsrmHttpClientCallHandler httpClientCallHandler) : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/release/releasesettings";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}