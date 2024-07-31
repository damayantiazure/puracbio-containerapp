using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

public class AddMemberRequest : HttpPostRequest<IDevHttpClientCallHandler, MembersGroupResponse, AddMemberData>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_identity/AddIdentities";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public AddMemberRequest(string organization, Guid projectId, AddMemberData addMemberData, IDevHttpClientCallHandler httpClientCallHandler)
        : base(addMemberData, httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}