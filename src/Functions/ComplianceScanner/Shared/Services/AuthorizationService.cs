using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Bits;
using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using User = Rabobank.Compliancy.Domain.Compliancy.Authorizations.User;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class AuthorizationService : IAuthorizationService
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IMemoryCache _cache;

    public AuthorizationService(IAzdoRestClient azdoClient, IMemoryCache cache)
    {
        _azdoClient = azdoClient;
        _cache = cache;
    }

    public async Task<bool> HasEditPermissionsAsync(HttpRequestMessage request, string organization,
        string projectId, string pipelineId, string pipelineType)
    {
        var user = await GetInteractiveUserAsync(request, organization);
        if (user.UniqueId == null)
        {
            throw new ArgumentException("Missing user Id");
        }

        if (pipelineType == ItemTypes.YamlReleasePipeline)
        {
            var buildPipeline = await _azdoClient.GetAsync(Builds.BuildDefinition(projectId, pipelineId),
                organization);

            if (buildPipeline == null)
            {
                throw new ItemNotFoundException(ErrorMessages.ItemNotFoundError);
            }

            return await HasEditBuildPermissions(organization, projectId, buildPipeline, Guid.Parse(user.UniqueId));
        }

        var classicRelease = await _azdoClient.GetAsync(ReleaseManagement.Definition(projectId, pipelineId),
            organization);

        if (classicRelease == null)
        {
            throw new ItemNotFoundException(ErrorMessages.ItemNotFoundError);
        }

        return await HasEditReleasePermissions(organization, projectId, classicRelease, Guid.Parse(user.UniqueId));
    }

    public async Task<User> GetInteractiveUserAsync(HttpRequestMessage request, string organization)
    {
        var accessToken = GetAccessTokenFromRequestHeader(request);

        ConnectionData connectionData = null;
        try
        {
            connectionData =
                await _azdoClient.GetWithTokenAsync(Connections.ConnectionData(), accessToken, organization);
        }
        catch (FlurlHttpException ex)
        {
            if (ex.Call.HttpStatus == HttpStatusCode.Unauthorized)
            {
                var response = await ex.GetResponseStringAsync();
                throw new TokenInvalidException($"Provided token is not valid or expired. {response}", ex);
            }
        }

        if (string.IsNullOrEmpty(connectionData?.AuthorizedUser?.Id))
        {
            throw new ArgumentException("Unable to retrieve user, UserId is empty.");
        }

        if (string.IsNullOrEmpty(connectionData.AuthorizedUser.Properties?.Account?.Value))
        {
            throw new ArgumentException("Unable to retrieve user, MailAddress is empty.");
        }

        return new User(connectionData.AuthorizedUser.ProviderDisplayName, connectionData.AuthorizedUser.Id)
        {
            MailAddress = connectionData.AuthorizedUser.Properties.Account.Value
        };
    }

    private async Task<bool> HasEditBuildPermissions(string organization, string projectId,
        BuildDefinition buildPipeline, Guid userId) =>
        await ManagePermissions
            .SetSecurityContextToSpecificBuildPipeline(_azdoClient, _cache, organization, projectId, buildPipeline.Id,
                buildPipeline.Path)
            .SetPermissionGroupTeamFoundationIdentifiers(userId)
            .SetPermissionsToBeInScope(BuildDefinitionBits.EditBuildPipeline)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Allow, PermissionLevelId.AllowInherited)
            .ValidateAsync();

    private async Task<bool> HasEditReleasePermissions(string organization, string projectId,
        ReleaseDefinition classicRelease, Guid userId) =>
        await ManagePermissions
            .SetSecurityContextToSpecificReleasePipeline(_azdoClient, _cache, organization, projectId,
                classicRelease.Id, classicRelease.Path)
            .SetPermissionGroupTeamFoundationIdentifiers(userId)
            .SetPermissionsToBeInScope(ReleaseDefinitionBits.EditReleasePipeline)
            .SetPermissionLevelIdsThatAreOkToHave(PermissionLevelId.Allow, PermissionLevelId.AllowInherited)
            .ValidateAsync();

    private static string GetAccessTokenFromRequestHeader(HttpRequestMessage request)
    {
        if (request?.Headers.Authorization == null || string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
        {
            // Throw the default exception. Users should not be able to differentiate between incorrect input or errors from Azdo API.
            throw new ArgumentException("Unable to retrieve user.");
        }

        return request.Headers.Authorization.Parameter;
    }
}