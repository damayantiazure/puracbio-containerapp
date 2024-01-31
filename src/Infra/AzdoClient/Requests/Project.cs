using System.Collections.Generic;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Project
{
    public static IAzdoRequest<Response.Project> ProjectById(string projectId, bool includeCapabilities = false) =>
        new AzdoRequest<Response.Project>($"_apis/projects/{projectId}", 
            new Dictionary<string, object>
            {
                {"api-version", "6.0"},
                {"includeCapabilities", includeCapabilities}
            });

    public static IEnumerableRequest<Response.Project> Projects() =>
        new AzdoRequest<Response.Project>(
            $"_apis/projects", new Dictionary<string, object>
            {
                {"$top", "1000"},
                {"api-version", "4.1-preview.2"}
            }).AsEnumerable();

    public static IAzdoRequest<ProjectProperties> Properties(string project) =>
        new AzdoRequest<ProjectProperties>(
            $"_apis/projects/{project}", new Dictionary<string, object>
            {
                {"api-version", "5.1-preview.2"}
            });

    public static IAzdoRequest<Response.Project> ProjectByName(string projectName) =>
        new AzdoRequest<Response.Project>(
            $"_apis/projects/{projectName}", new Dictionary<string, object>
            {
                {"api-version", "5.0"}
            });

    public static IAzdoRequest<Response.Project, OperationReference> CreateProject() =>
        new AzdoRequest<Response.Project, OperationReference>(
            $"_apis/projects", new Dictionary<string, object>
            {
                {"api-version", "6.0"}
            });

    public static IAzdoRequest<Response.OperationReference> Operation(string operationId) =>
        new AzdoRequest<Response.OperationReference>(
            $"_apis/operations/{operationId}", new Dictionary<string, object>
            {
                {"api-version", "6.1-preview.1"}
            });
}