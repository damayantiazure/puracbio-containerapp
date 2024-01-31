#nullable enable

using Microsoft.TeamFoundation.Core.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Extensions;
using Rabobank.Compliancy.Infrastructure.Parsers;
using System.Globalization;
using System.Net;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc/>
public class ProjectService : IProjectService
{
    private const string _projectNotFoundError = "Could not find project {0} for organization {1}";
    private readonly IProjectRepository _projectRepository;
    private readonly IAccessControlListsRepository _accessControlListsRepository;
    private readonly IRecursiveIdentityCacheBuilder _recursiveIdentityCacheBuilder;

    private static readonly Dictionary<Guid, IEnumerable<AccessControlList>?> _cachedAccessControlLists = new();

    public ProjectService(IProjectRepository projectRepository, IAccessControlListsRepository accessControlListsRepository, IRecursiveIdentityCacheBuilder recursiveIdentityCacheBuilder)
    {
        _projectRepository = projectRepository;
        _accessControlListsRepository = accessControlListsRepository;
        _recursiveIdentityCacheBuilder = recursiveIdentityCacheBuilder;
    }

    /// <inheritdoc/>
    public async Task<Project> GetProjectByIdAsync(string organization, Guid projectId, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectReference = await _projectRepository.GetProjectByIdAsync(organization, projectId, false, cancellationToken);
            
            return projectReference == null
                ? throw new SourceItemNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, _projectNotFoundError, projectId, organization))
                : CreateProject(organization, projectReference);
        }
        catch (HttpRequestException ex)
        {
            HandleHttpException(ex, projectId.ToString(), organization);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project> GetProjectByNameAsync(string organization, string projectName, CancellationToken cancellationToken = default)
    {
        try
        {
            var projectReference = await _projectRepository.GetProjectByNameAsync(organization, projectName, false, cancellationToken);
            
            return projectReference == null
                ? throw new SourceItemNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, _projectNotFoundError, projectName, organization))
                : CreateProject(organization, projectReference);
        }
        catch (HttpRequestException ex)
        {
            HandleHttpException(ex, projectName, organization);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<Project> AddPermissionsAsync(Project project, CancellationToken cancellationToken = default)
    {
        var projectPermissions = await GetCachedProjectSecurityNamespaceAsync(project.Organization, project.Id, cancellationToken);
        var identitiesAllowedList = new List<IdentityDescriptor>();
        var identitiesDeniedList = new List<IdentityDescriptor>();

        foreach (var acesDictionary in projectPermissions.SelectMany(permission => permission.AcesDictionary))
        {
            identitiesAllowedList.AddIf(PermissionsBitFlagsParser.IsEnumFlagPresent(acesDictionary.Value.ExtendedInfo?.EffectiveAllow, ProjectPermissionBits.DELETE), acesDictionary.Key);
            identitiesDeniedList.AddIf(PermissionsBitFlagsParser.IsEnumFlagPresent(acesDictionary.Value.Deny, ProjectPermissionBits.DELETE), acesDictionary.Key); // Only explicit deny (because explicit deny > all allow, inherited deny < explicit allow)
        }

        var identityParser = new IdentityParser(_recursiveIdentityCacheBuilder);

        var allowedIdentities = await identityParser.ParseIdentityDescriptors(project.Organization, identitiesAllowedList, cancellationToken);
        var deniedIdentityIds = (await identityParser.ParseIdentityDescriptors(project.Organization, identitiesDeniedList, cancellationToken))
            .SelectMany(identity => identity.MapIdentitiesHierarchy()).Select(identity => identity.UniqueId).Distinct();

        var allowedIdentitiesThatAreNotExplicitlyDenied = allowedIdentities.NotExplicitDenied(deniedIdentityIds);

        RemoveProjectAdministratorsGroupIfEmpty(allowedIdentitiesThatAreNotExplicitlyDenied, project.Name);

        if (allowedIdentitiesThatAreNotExplicitlyDenied.Any())
        {
            project.Permissions.Add
            (
                Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes.ProjectMisUse.Delete,
                allowedIdentitiesThatAreNotExplicitlyDenied
            );
        }

        return project;
    }

    private static Project CreateProject(string organization, TeamProjectReference projectReference) =>
        new()
        {
            Id = projectReference.Id,
            Name = projectReference.Name,
            Organization = organization
        };

    private static void HandleHttpException(HttpRequestException ex, string project, string organization)
    {
        switch (ex.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new SourceItemNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, _projectNotFoundError, project, organization),
                    ex);
        }
    }

    private async Task<IEnumerable<AccessControlList>> GetCachedProjectSecurityNamespaceAsync(
        string organization, Guid projectId, CancellationToken cancellationToken)
    {
        if (_cachedAccessControlLists.TryGetValue(projectId, out var cachedAccessControlListsByProject))
        {
            return cachedAccessControlListsByProject ?? Enumerable.Empty<AccessControlList>();
        }

        var accessControlLists =
            (await _accessControlListsRepository.GetAccessControlListsForProjectAndSecurityNamespaceAsync(organization,
                projectId, SecurityNamespaces.Project, cancellationToken))?.ToList();

        _cachedAccessControlLists.TryAdd(projectId, accessControlLists);

        return accessControlLists ?? Enumerable.Empty<AccessControlList>();
    }

    private static bool RemoveProjectAdministratorsGroupIfEmpty(ICollection<IIdentity> identities, string projectName)
    {
        var projectAdministratorsFound = false;
        foreach (var identity in identities)
        {
            if (identity is not Group identityAsGroup)
            {
                continue;
            }

            if (identityAsGroup.DisplayName.GroupNameEquals(projectName, "Project Administrators"))
            {
                projectAdministratorsFound = true;
                if (!identityAsGroup.GetMembers().Any())
                {
                    identities.Remove(identity);
                }
                break;
            }

            projectAdministratorsFound = RemoveProjectAdministratorsGroupIfEmpty(identityAsGroup.GetMembers(), projectName);
            if (projectAdministratorsFound)
            {
                break;
            }
        }
        return projectAdministratorsFound;
    }
}