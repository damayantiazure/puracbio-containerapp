using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.ApplicationGroup;

/// <summary>
/// NOTE: This endpoint is a legacy endpoint of microsoft and needs to be replaced with a
/// more recent endpoint that is documented on the azure devops api website. <see cref="https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.1"/>
/// </summary>
internal class GetApplicationGroupsForGroupRequest : HttpGetRequest<IDevHttpClientCallHandler, ResponseCollection<Models.ApplicationGroup>>
{
    private readonly string _organization;
    private readonly Guid _groupId;

    protected override string Url => $"{_organization}/_api/_identity/ReadGroupMembers";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"__v", "5"},
        {"scope", $"{_groupId}"}
    };

    public GetApplicationGroupsForGroupRequest(string organization, Guid groupId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _groupId = groupId;
    }
}