using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Graph.Client;
using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Identities;

public class GetIdentitiesForIdentityDescriptorsRequest : GetIdentitiesRequest
{
    public GetIdentitiesForIdentityDescriptorsRequest(
        string organization,
        IEnumerable<IdentityDescriptor> identityDescriptors,
        QueryMembership? queryMembership,
        IVsspsHttpClientCallHandler callHandler)
        : base(
            organization,
            identityDescriptors,
            Enumerable.Empty<SubjectDescriptor>(),
            Enumerable.Empty<GraphStorageKeyResult>(),
            queryMembership,
            callHandler)
    {
    }
}