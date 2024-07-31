using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.ApplicationGroup;

internal class GetApplicationGroupForRepositoryRequest : HttpGetRequest<IDevHttpClientCallHandler, Models.ApplicationGroup>
{
    private readonly string _organization;
    private readonly Guid _projectId;
    private readonly Guid _permissionSetId = SecurityNamespaces.GitRepo;
    private readonly Guid _repositoryId;

    protected override string Url => $"{_organization}/{_projectId}/_api/_security/ReadExplicitIdentitiesJson";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "__v", "5" },
        { "permissionSetId", $"{_permissionSetId}" },
        { "permissionSetToken", $"repoV2/{_projectId}/{_repositoryId}" }
    };

    public GetApplicationGroupForRepositoryRequest(string organization, Guid projectId, Guid repositoryId, IDevHttpClientCallHandler httpClientCallHandler)
        : base(httpClientCallHandler)
    {
        _organization = organization;
        _projectId = projectId;
        _repositoryId = repositoryId;
    }
}