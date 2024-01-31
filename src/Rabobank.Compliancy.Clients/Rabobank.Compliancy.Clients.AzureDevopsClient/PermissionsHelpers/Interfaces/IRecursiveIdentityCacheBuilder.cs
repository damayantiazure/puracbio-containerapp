using Microsoft.VisualStudio.Services.Identity;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;

public interface IRecursiveIdentityCacheBuilder
{
    Task<IEnumerable<Identity>> GetIdentitiesFromCacheAsync(string organization, IEnumerable<IdentityDescriptor> identityDescriptors, CancellationToken cancellationToken = default);
}