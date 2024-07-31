#nullable enable

using Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;

namespace Rabobank.Compliancy.Infrastructure.InternalServices;

public class PermissionGroupService : IPermissionGroupService
{
    private readonly IApplicationGroupRepository _applicationGroupRepository;

    public PermissionGroupService(IApplicationGroupRepository applicationGroupRepository)
    {
        _applicationGroupRepository = applicationGroupRepository;
    }

    public async Task<IEnumerable<Guid>> GetUniqueIdentifiersForNativeAzdoGroups<TProtectedResource>(IPermissionContextForResource<TProtectedResource> context,
        CancellationToken cancellationToken)
        where TProtectedResource : IProtectedResource
    {
        var organization = context.Resource.Project.Organization;
        var projectId = context.Resource.Project.Id;
        var allPermissionGroupsForProjectCollection = await _applicationGroupRepository
            .GetScopedApplicationGroupForProjectAsync(organization, projectId, cancellationToken);
        var allPermissionGroupsForProject =
            allPermissionGroupsForProjectCollection?.Identities ?? Enumerable.Empty<PermissionGroup>();

        var nativeAzdoPermissionGroupIdentifiers = allPermissionGroupsForProject?
            .Where(group =>
                context.GetNativeAzureDevOpsSecurityDisplayNames()
                    .Contains(group.FriendlyDisplayName,
                        StringComparer.InvariantCultureIgnoreCase))
            .Select(group => group.TeamFoundationId)
            .ToList();

        return nativeAzdoPermissionGroupIdentifiers ?? Enumerable.Empty<Guid>();
    }
}