using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class ReleaseManagement
{
    public static IAzdoRequest<Response.Release> Release(string project, string id) =>
        new VsrmRequest<Response.Release>($"{project}/_apis/release/releases/{id}");

    public static IEnumerableRequest<Response.Release> Releases(string project) =>
        new VsrmRequest<Response.Release>($"{project}/_apis/release/releases").AsEnumerable();

    public static IEnumerableRequest<Response.Release> Releases(string project, string expand, string asof) =>
        new VsrmRequest<Response.Release>($"{project}/_apis/release/releases", new Dictionary<string, object>
        {
            { "$expand", $"{expand}" },
            { "minCreatedTime", $"{asof}" }
        }).AsEnumerable();

    public static IEnumerableRequest<Response.Release> Releases(string project,
        string pipelineId, string expand, string asof) =>
        new VsrmRequest<Response.Release>($"{project}/_apis/release/releases",
            new Dictionary<string, object>
            {
                { "definitionId", pipelineId },
                { "$expand", expand },
                { "minCreatedTime", asof },
                { "api-version", "5.1" }
            }).AsEnumerable();

    public static IAzdoRequest<Response.ReleaseDefinition> Definition(string project, string id) =>
        new VsrmRequest<Response.ReleaseDefinition>($"{project}/_apis/release/definitions/{id}");

    public static IAzdoRequest<Response.ReleaseDefinition> Definition(string project, string id, 
        string pipelineVersion) =>
        new VsrmRequest<Response.ReleaseDefinition>(
            $"{project}/_apis/release/definitions/{id}", new Dictionary<string, object>
            {
                {"revision", pipelineVersion },
                {"api-version", "6.0-preview.1"}
            });

    public static IEnumerableRequest<Response.ReleaseDefinition> Definitions(string project) =>
        new VsrmRequest<Response.ReleaseDefinition>($"{project}/_apis/release/definitions/").AsEnumerable();

    public static IEnumerableRequest<Response.ReleaseDefinition> Definitions(string project, string expand) =>
        new VsrmRequest<Response.ReleaseDefinition>($"{project}/_apis/release/definitions/", new Dictionary<string, object>
        {
            { "$expand", $"{expand}" }
        }).AsEnumerable();

    public static IEnumerableRequest<Response.ReleaseApproval> Approvals(
        string project, string releaseId, string status) =>
        new VsrmRequest<Response.ReleaseApproval>($"{project}/_apis/release/approvals",
            new Dictionary<string, object>
            {
                { "releaseIdsFilter", releaseId },
                { "statusFilter", status },
                { "api-version", "5.1" }
            }).AsEnumerable();

    public static IAzdoRequest<Response.ReleaseSettings> Settings(string project) =>
        new VsrmRequest<Response.ReleaseSettings>($"{project}/_apis/release/releasesettings", new Dictionary<string, object>
        {
            { "api-version", "5.0-preview" }
        });

    public static IAzdoRequest<Response.Tags> Tag(string project, string id, string tag) =>
        new VsrmRequest<Response.Tags>($"{project}/_apis/release/releases/{id}/tags/{tag}", 
            new Dictionary<string, object>
            {
                {"api-version", "6.0-preview"}
            });

    public static IAzdoRequest<Response.Tags> Tags(string project, string id) =>
        new VsrmRequest<Response.Tags>($"{project}/_apis/release/releases/{id}/tags",
            new Dictionary<string, object>
            {
                {"api-version", "6.0-preview"}
            });

    public static IAzdoRequest TaskLogs(string project, int releaseId, int environmentId, int releaseDeployPhaseId, string taskId) =>
        new VsrmRequest<object>($"{project}/_apis/release/releases/{releaseId}/environments/{environmentId}/deployPhases/{releaseDeployPhaseId}/tasks/{taskId}/logs",
            new Dictionary<string, object>
            {
                {"api-version", "7.0"}
            });
}