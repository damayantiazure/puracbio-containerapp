using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

public class CreateManagedGroupRequest : HttpPostRequest<IDevHttpClientCallHandler, Group, ManageGroup>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_identity/ManageGroup";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };

    public CreateManagedGroupRequest(string organization, Guid projectId, string? groupName
        , IDevHttpClientCallHandler httpClientCallHandler) : base(new ManageGroup { Name = groupName }, httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
    }
}