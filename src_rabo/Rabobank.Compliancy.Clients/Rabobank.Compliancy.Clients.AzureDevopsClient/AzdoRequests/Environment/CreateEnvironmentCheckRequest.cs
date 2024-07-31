using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Environment;

public class CreateEnvironmentCheckRequest : HttpPostRequest<IDevHttpClientCallHandler,
    CheckConfiguration, EnvironmentCheckBodyContent>
{
    private readonly string _organization;
    private readonly Guid _projectId;

    public CreateEnvironmentCheckRequest(string organization, Guid projectId, EnvironmentCheckBodyContent value,
        IDevHttpClientCallHandler callhandler) : base(value, callhandler)
    {
        _organization = organization;
        _projectId = projectId;
    }

    protected override string Url => $"{_organization}/{_projectId}/_apis/pipelines/checks/configurations";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" }
    };
}