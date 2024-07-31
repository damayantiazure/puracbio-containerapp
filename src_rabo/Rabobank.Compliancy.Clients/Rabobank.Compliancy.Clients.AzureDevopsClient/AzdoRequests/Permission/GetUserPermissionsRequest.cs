using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission;

/// <summary>
/// NOTE: This endpoint is a legacy endpoint of microsoft and needs to be replaced with a
/// more recent endpoint that is documented on the azure devops api website. <see cref="https://learn.microsoft.com/en-us/rest/api/azure/devops/?view=azure-devops-rest-7.1"/>
/// </summary>
public class GetUserPermissionsRequest : HttpGetRequest<IDevHttpClientCallHandler, PermissionsProjectId>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly Guid _applicationGroupId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_identity/Display";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        {"__v", "5"},
        {"tfid", _applicationGroupId.ToString()}
    };

    public GetUserPermissionsRequest(string organization, Guid projectId, Guid applicationGroupId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _applicationGroupId = applicationGroupId;
    }
}