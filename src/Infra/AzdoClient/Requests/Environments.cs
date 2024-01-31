using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Environments
{
    public static IEnumerableRequest<EnvironmentYaml> All(
        string projectId) =>
        new AzdoRequest<EnvironmentYaml>(
            $"{projectId}/_apis/distributedtask/environments", new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            }).AsEnumerable();

    public static IEnumerableRequest<EnvironmentConfiguration> Configuration(
        string projectId,
        int environmentId) =>
        new AzdoRequest<EnvironmentConfiguration>(
            $"{projectId}/_apis/pipelines/checks/configurations", new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"},
                {"resourceType", "environment"},
                {"resourceId", environmentId}
            }).AsEnumerable();

    public static IEnumerableRequest<EnvironmentSecurityGroup> Security(
        string projectId,
        int environmentId) =>
        new AzdoRequest<EnvironmentSecurityGroup>(
            "_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources" +
            $"/{projectId}_{environmentId}", new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            }).AsEnumerable();

    public static IAzdoRequest<object, EnvironmentSecurityGroup> UpdateSecurity(
        string projectId, int environmentId) =>
        new AzdoRequest<object, EnvironmentSecurityGroup>(
            "_apis/securityroles/scopes/distributedtask.environmentreferencerole/roleassignments/resources" +
            $"/{projectId}_{environmentId}", new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            });

    public static IAzdoRequest<JObject> Checks(
        string projectId,
        int environmentId) =>
        new AzdoRequest<JObject>(
            $"{projectId}/_environments/{environmentId}/checks",
            new Dictionary<string, object>
            {
                {"__rt", "fps"},
                {"__ver", "2"},
                {"api-version", "6.0-preview.1"}
            });

    public static IAzdoRequest DeleteCheck(
        string projectId,
        string configId) =>
        new AzdoRequest(
            $"{projectId}/_apis/pipelines/checks/configurations/{configId}",
            new Dictionary<string, object>
            {
                {"api-version", "7.0-preview.1"}
            });

    public static IAzdoRequest<object, JObject> CreateCheck(
        string projectId) =>
        new AzdoRequest<object, JObject>(
            $"{projectId}/_apis/pipelines/checks/configurations",
            new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            });

    public static object CreateCheckBody(string functionHostName, string environmentName, int environmentId) =>
        new
        {
            type = new
            {
                id = "fe1de3ee-a436-41b4-bb20-f6eb4cb879a7",
                name = "Task Check"
            },
            settings = new
            {
                definitionRef = new
                {
                    id = "537fdb7a-a601-4537-aa70-92645a2b5ce4",
                    name = "AzureFunction",
                    version = "1.0.12"
                },
                displayName = "4-eyes principle check",
                inputs = new
                {
                    method = "POST",
                    waitForCompletion = "true",
                    function =
                        $"https://{functionHostName}/api/validate-yaml-approvers/$(System.TeamProjectId)/$(Build.BuildId)",
                    key = "not-required"
                },
                retryInterval = 0,
                linkedVariableGroup = (string)null
            },
            resource = new
            {
                type = "environment",
                id = environmentId.ToString(CultureInfo.InvariantCulture),
                name = environmentName
            },
            timeout = 180
        };

    public static object[] CreateUpdateSecurityBody(string userId, string roleName) =>
        new[] { new { userId, roleName } };
}