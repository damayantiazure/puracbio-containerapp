using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Identities;

/// <summary>
/// Canonical name: Identities - Read Identities
/// Documentation: https://learn.microsoft.com/en-us/rest/api/azure/devops/ims/identities/read-identities
/// Least privileges:
/// - ?
/// </summary>
public class GetIdentitiesRequest : HttpGetRequest<IVsspsHttpClientCallHandler, ResponseCollection<Identity>>
{
    private readonly string _organization;
    private readonly QueryMembership _queryMembership;
    private readonly IEnumerable<IdentityDescriptor> _strongIdentityDescriptors;
    private readonly IEnumerable<SubjectDescriptor> _strongSubjectDescriptors;
    private readonly IEnumerable<GraphStorageKeyResult> _strongGraphStorageKeys;

    protected override string Url => $"{_organization}/_apis/identities";

    protected override Dictionary<string, string> QueryStringParameters => new()
    {
        { "api-version", "7.0" },
        { "descriptors", string.Join(",", _strongIdentityDescriptors) },
        { "identityIds", string.Join(",", _strongGraphStorageKeys) },
        { "queryMembership", _queryMembership.ToString() },
        { "subjectDescriptors", string.Join(",", _strongSubjectDescriptors) }
    };

    public GetIdentitiesRequest(
        string organization,
        IEnumerable<IdentityDescriptor> identityDescriptors,
        IEnumerable<SubjectDescriptor> subjectDescriptors,
        IEnumerable<GraphStorageKeyResult> graphStorageKeys,
        QueryMembership? queryMembership,
        IVsspsHttpClientCallHandler callHandler)
        : base(callHandler)
    {
        _organization = organization;
        _strongIdentityDescriptors = identityDescriptors;
        _strongSubjectDescriptors = subjectDescriptors;
        _strongGraphStorageKeys = graphStorageKeys;
        _queryMembership = queryMembership ?? QueryMembership.None;
    }
}