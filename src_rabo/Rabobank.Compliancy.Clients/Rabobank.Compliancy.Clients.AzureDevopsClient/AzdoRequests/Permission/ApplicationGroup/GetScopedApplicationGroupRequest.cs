using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.ApplicationGroup;

/// <summary>
/// NOTE: This endpoint is a legacy endpoint of microsoft and needs to be replaced with a
/// more recent endpoint that is documented on the azure devops api website.
/// <see cref="https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.1"/>
/// </summary>
internal class GetScopedApplicationGroupRequest : HttpGetRequest<IDevHttpClientCallHandler, Models.ApplicationGroup>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_identity/ReadScopedApplicationGroupsJson";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "__v", "5" }
    };

    public GetScopedApplicationGroupRequest(string organization, Guid projectId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}