using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class ApplicationGroup
{
    public static IAzdoRequest<Response.ApplicationGroups> ApplicationGroups() =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"_api/_identity/ReadScopedApplicationGroupsJson", new Dictionary<string, object>
            {
                {"__v", "5"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ApplicationGroups(string project) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"{project}/_api/_identity/ReadScopedApplicationGroupsJson", new Dictionary<string, object>
            {
                {"__v", "5"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> GroupMembers(string project, string groupId) =>
        new AzdoRequest<Response.ApplicationGroups>($"/{project}/_api/_identity/ReadGroupMembers",
            new Dictionary<string, object>
            {
                {"__v", "5"},
                {"scope", groupId},
                {"readMembers", "true"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesRepos(string projectId,
        string securityNamespaceId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"repoV2/{projectId}"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesRepos(string projectId,
        string securityNamespaceId, string repositoryId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"repoV2/{projectId}/{repositoryId}"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesMasterBranch(string projectId,
        string securityNamespaceId, string repositoryId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"repoV2/{projectId}/{repositoryId}/refs/heads/master"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesPipelines(string projectId,
        string securityNamespaceId, string pipelineId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"{projectId}/{pipelineId}"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesPipelines(string projectId,
        string securityNamespaceId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"{projectId}"}
            });

    public static IAzdoRequest<Response.ApplicationGroups> ExplicitIdentitiesPipelineStage(string projectId,
        string securityNamespaceId, string pipelineId, string stageId) =>
        new AzdoRequest<Response.ApplicationGroups>(
            $"/{projectId}/_api/_security/ReadExplicitIdentitiesJson", new Dictionary<string, object>
            {
                {"__v", "5"},
                {"permissionSetId", securityNamespaceId},
                {"permissionSetToken", $"{projectId}/{pipelineId}/Environment/{stageId}"}
            });
}