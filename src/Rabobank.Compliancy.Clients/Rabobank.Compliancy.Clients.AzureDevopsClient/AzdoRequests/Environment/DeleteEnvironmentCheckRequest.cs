using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

public class DeleteEnvironmentCheckRequest : HttpDeleteRequest<IDevHttpClientCallHandler>
{
    private readonly string _id;
    private readonly string _organization;
    private readonly Guid _projectId;

    public DeleteEnvironmentCheckRequest(string organization, Guid projectId, string id, IDevHttpClientCallHandler callHandler) :
        base(callHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _id = id;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/checks/configurations/{_id}";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}