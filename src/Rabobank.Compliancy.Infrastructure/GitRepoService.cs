#nullable enable

using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Microsoft.VisualStudio.Services.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exceptions;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.PermissionFlags;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Parsers;
using System.Globalization;
using System.Net;

namespace Rabobank.Compliancy.Infrastructure;

/// <inheritdoc/>
public class GitRepoService : IGitRepoService
{
    private const string _gitRepoNotFoundError = "Could not find repo {0} in project {1} for organization {2}";
    private const string _noReposFoundError = "Could not find any GitRepos for project {0} for organization {1}";
    private readonly IGitRepositoryRepository _gitRepoRepository;
    private readonly IAccessControlListsRepository _accessControlListsRepository;
    private readonly IRecursiveIdentityCacheBuilder _recursiveIdentityCacheBuilder;

    private readonly Dictionary<Guid, IEnumerable<GitRepo>> _cachedGitRepos = new();

    private static readonly Dictionary<Guid, IEnumerable<AccessControlList>?> _cachedAccessControlLists = new();

    public GitRepoService(IGitRepositoryRepository gitRepoRepository, IAccessControlListsRepository accessControlListsRepository, IRecursiveIdentityCacheBuilder recursiveIdentityCacheBuilder)
    {
        _gitRepoRepository = gitRepoRepository;
        _accessControlListsRepository = accessControlListsRepository;
        _recursiveIdentityCacheBuilder = recursiveIdentityCacheBuilder;
    }

    /// <inheritdoc/>
    public async Task<GitRepo> GetGitRepoByIdAsync(Project project, Guid gitRepoId, CancellationToken cancellationToken = default)
    {
        try
        {
            var reposForThisProject = await GetGitRepoCacheByProject(project, cancellationToken);
            var gitRepository = reposForThisProject.FirstOrDefault(r => r.Id == gitRepoId);
            return gitRepository ?? throw new SourceItemNotFoundException(_gitRepoNotFoundError, gitRepoId, project.Name, project.Organization);
        }
        catch (HttpRequestException ex)
        {
            HandleHttpException(ex, gitRepoId, project.Name, project.Organization);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<GitRepo> GetGitRepoByNameAsync(Project project, string gitRepoName, CancellationToken cancellationToken = default)
    {
        try
        {
            var reposForThisProject = await GetGitRepoCacheByProject(project, cancellationToken);
            var gitRepository = reposForThisProject.FirstOrDefault(r => r.Name == gitRepoName);
            return gitRepository ?? throw new SourceItemNotFoundException(_gitRepoNotFoundError, gitRepoName, project.Name, project.Organization);
        }
        catch (HttpRequestException ex)
        {
            HandleHttpException(ex, gitRepoName, project.Name, project.Organization);
            throw;
        }
    }

    public async Task<GitRepo> AddPermissionsAsync(GitRepo gitRepo, CancellationToken cancellationToken = default)
    {
        var gitRepoPermissions = await GetCachedGitRepoSecurityNamespaceAsync(gitRepo.Project.Organization, gitRepo.Project.Id, cancellationToken);
        var identitiesAllowedToManageList = new List<IdentityDescriptor>();
        var identitiesAllowedToDeleteList = new List<IdentityDescriptor>();

        foreach (var acesDictionary in gitRepoPermissions.SelectMany(permission => permission.AcesDictionary))
        {
            identitiesAllowedToManageList.AddIf(PermissionsBitFlagsParser.IsEnumFlagPresent(acesDictionary.Value.ExtendedInfo?.EffectiveAllow, GitRepositoryPermissionBits.ManagePermissions), acesDictionary.Key);
            identitiesAllowedToDeleteList.AddIf(PermissionsBitFlagsParser.IsEnumFlagPresent(acesDictionary.Value.ExtendedInfo?.EffectiveAllow, GitRepositoryPermissionBits.DeleteRepository), acesDictionary.Key);
        }

        var identityParser = new IdentityParser(_recursiveIdentityCacheBuilder);

        if (identitiesAllowedToManageList.Any())
        {
            gitRepo.Permissions.Add
            (
                RepositoryMisUse.Manage,
                await identityParser.ParseIdentityDescriptors(gitRepo.Project.Organization, identitiesAllowedToManageList, cancellationToken)
            );
        }

        if (identitiesAllowedToDeleteList.Any())
        {
            gitRepo.Permissions.Add
            (
                RepositoryMisUse.Delete,
                await identityParser.ParseIdentityDescriptors(gitRepo.Project.Organization, identitiesAllowedToDeleteList, cancellationToken)
            );
        }

        return gitRepo;
    }

    private async Task<IEnumerable<AccessControlList>> GetCachedGitRepoSecurityNamespaceAsync(string organization, Guid projectId, CancellationToken cancellationToken)
    {
        if (_cachedAccessControlLists.TryGetValue(projectId, out var cachedAccessControlListsByProject))
        {
            return cachedAccessControlListsByProject ?? Enumerable.Empty<AccessControlList>();
        }

        var accessControlLists =
            (await _accessControlListsRepository.GetAccessControlListsForProjectAndSecurityNamespaceAsync(organization,
                projectId, SecurityNamespaces.GitRepo, cancellationToken))?.ToList();
        
        _cachedAccessControlLists.TryAdd(projectId, accessControlLists);

        return accessControlLists ?? Enumerable.Empty<AccessControlList>();
    }

    private async Task<IEnumerable<GitRepo>> GetGitRepoCacheByProject(Project project, CancellationToken cancellationToken = default)
    {
        if (_cachedGitRepos.TryGetValue(project.Id, out var cachedGitReposByProject))
        {
            return cachedGitReposByProject;
        }

        var gitRepos = (await _gitRepoRepository.GetGitReposByProjectAsync(project.Organization, project.Id, cancellationToken))
                       ?? throw new UnexpectedDataException(_noReposFoundError, project.Name, project.Organization);

        _cachedGitRepos.TryAdd(project.Id, gitRepos.Select(gitRepository => new GitRepo
            {
                Id = gitRepository.Id,
                Name = gitRepository.Name,
                Project = project,
                Url = new Uri(gitRepository.Url)
            }
        ));

        return _cachedGitRepos[project.Id];
    }

    private static void HandleHttpException(HttpRequestException ex, params object[] parameters)
    {
        switch (ex.StatusCode)
        {
            case HttpStatusCode.NotFound:
                throw new SourceItemNotFoundException(
                    string.Format(CultureInfo.InvariantCulture, _gitRepoNotFoundError, parameters),
                    ex);
        }
    }
}