using Rabobank.Compliancy.Infra.AzdoClient.Permissions.Constants;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Permissions
{
    public static IAzdoRequest<PermissionsSet> PermissionsGroupRepository(string projectId, string applicationGroupId, string repositoryId)
    {
        return new AzdoRequest<PermissionsSet>(
            $"{projectId}/_api/_security/DisplayPermissions", new Dictionary<string, object>
            {
                {"__v","5"},
                {"tfid", applicationGroupId},
                {"permissionSetId", SecurityNamespaceIds.GitRepositories},
                {"permissionSetToken", $"repoV2/{projectId}/{repositoryId}"}
            });
    }

    public static IAzdoRequest<PermissionsSet> PermissionsGroupSetId(string projectId, string permissionSetId, string applicationGroupId)
    {
        return new AzdoRequest<PermissionsSet>(
            $"{projectId}/_api/_security/DisplayPermissions", new Dictionary<string, object>
            {
                {"__v","5"},
                {"tfid", applicationGroupId},
                {"permissionSetId", permissionSetId},
                {"permissionSetToken", projectId}
            });
    }

    public static IAzdoRequest<PermissionsSet> PermissionsGroupSetIdDefinition(string projectId,
        string permissionSetId, string applicationGroupId, string permissionSetToken)
    {
        return new AzdoRequest<PermissionsSet>(
            $"{projectId}/_api/_security/DisplayPermissions", new Dictionary<string, object>
            {
                {"__v","5"},
                {"tfid", applicationGroupId},
                {"permissionSetId", permissionSetId},
                {"permissionSetToken", permissionSetToken}
            });
    }

    /// <summary>
    /// Gets project permissions for an applicationGroup
    /// </summary>
    /// <param name="projectId"></param>
    /// <param name="applicationGroupId"></param>
    /// <returns></returns>
    public static IAzdoRequest<PermissionsProjectId> PermissionsGroupProjectId(string projectId, string applicationGroupId)
    {
        return new AzdoRequest<PermissionsProjectId>(
            $"{projectId}/_api/_identity/Display", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"tfid", applicationGroupId}
            });
    }

    /// <summary>
    /// But ugly REST API where this wrapper is required and only has one property with JSON serialized content
    /// </summary>
    public class UpdateWrapper
    {
        public string UpdatePackage { get; }

        public UpdateWrapper(string content)
        {
            UpdatePackage = content;
        }
    }

    public static IAzdoRequest<UpdateWrapper, object> ManagePermissions(string project) =>
        new AzdoRequest<UpdateWrapper, object>($"{project}/_api/_security/ManagePermissions", new Dictionary<string, object>
        {
            {"__v", "5"}
        });
}