using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

public interface IIdentityRepository
{
    /// <summary>
    /// Gets the <see cref="Identity"/> for a given set of <see cref="IdentityDescriptor"/>
    /// </summary>
    /// <param name="organization">Organization in Scope</param>
    /// <param name="identityDescriptors">List of <see cref="IdentityDescriptor"/> for which the <see cref="Identity"/> should be retrieved</param>
    /// <param name="queryMembership">Defines whether any memberhip information should be retrieved. Returns <see cref="string"/> instances of <see cref="IdentityDescriptor"/></param>
    /// <param name="cancellationToken">Cancels the API call if necessary</param>
    /// <returns></returns>
    Task<IEnumerable<Identity>?> GetIdentitiesForIdentityDescriptorsAsync(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, QueryMembership queryMembership, CancellationToken cancellationToken = default);
}