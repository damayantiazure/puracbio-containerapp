using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class Builds
{
    public static IEnumerableRequest<BuildDefinition> BuildDefinitions(
        string projectId, bool includeAllProperties, int processType) =>
        new AzdoRequest<BuildDefinition>(
            $"{projectId}/_apis/build/definitions", new Dictionary<string, object>
            {
                {"includeAllProperties", $"{includeAllProperties}"},
                {"processType", $"{processType}"},
                {"api-version", "5.0-preview.7"}
            }).AsEnumerable();
        
    public static IEnumerableRequest<BuildDefinition> BuildDefinitions(string projectId, bool includeAllProperties) =>
        new AzdoRequest<BuildDefinition>(
            $"{projectId}/_apis/build/definitions", new Dictionary<string, object>
            {
                {"includeAllProperties", $"{includeAllProperties}"},
                {"api-version", "5.0-preview.7"}
            }).AsEnumerable();

    public static IEnumerableRequest<BuildDefinition> BuildDefinitions(string projectId, int processType) =>
        BuildDefinitions(projectId, false, processType);

    public static IEnumerableRequest<BuildDefinition> BuildDefinitions(string projectId) =>
        BuildDefinitions(projectId, false);

    public static IAzdoRequest<BuildDefinition> BuildDefinition(string projectId, string id) =>
        new AzdoRequest<BuildDefinition>($"{projectId}/_apis/build/definitions/{id}");

    public static IAzdoRequest<BuildDefinition> BuildDefinition(
        string projectId, string pipelineId, string pipelineVersion) =>
        new AzdoRequest<BuildDefinition>(
            $"{projectId}/_apis/build/definitions/{pipelineId}", new Dictionary<string, object>
            {
                {"revision", pipelineVersion },
                {"api-version", "7.1-preview.7"}
            });

    public static IEnumerableRequest<BuildArtifact> Artifacts(string project, string id) =>
        new AzdoRequest<BuildArtifact>($"{project}/_apis/build/builds/{id}/artifacts").AsEnumerable();

    public static IEnumerableRequest<Change> Changes(string project, string id) =>
        new AzdoRequest<Change>($"{project}/_apis/build/builds/{id}/changes", 60).AsEnumerable();

    public static IAzdoRequest<Timeline> Timeline(string project, string id) =>
        new AzdoRequest<Timeline>($"{project}/_apis/build/builds/{id}/timeline");

    public static IAzdoRequest<Build> Build(string project, string id) =>
        new AzdoRequest<Build>($"{project}/_apis/build/builds/{id}", 60);

    public static IEnumerableRequest<Build> LongRunningBuilds(string project, string queryOrder, string minTime) =>
        new AzdoRequest<Build>($"{project}/_apis/build/builds/", new Dictionary<string, object>
        {
            {"queryOrder", queryOrder},
            { "minTime", minTime},
            {"api-version", "5.1"}
        }).AsEnumerable();

    public static IEnumerableRequest<Build> All(string project) =>
        new AzdoRequest<Build>($"{project}/_apis/build/builds").AsEnumerable();

    public static IAzdoRequest<ProjectRetentionSetting> Retention(string project) =>
        new AzdoRequest<ProjectRetentionSetting>($"{project}/_apis/build/retention", 
            new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            });

    public static IAzdoRequest<SetRetention, ProjectRetentionSetting> SetRetention(string project) =>
        new AzdoRequest<SetRetention, ProjectRetentionSetting>($"{project}/_apis/build/retention",
            new Dictionary<string, object>
            {
                {"api-version", "6.0-preview.1"}
            });

    public static IAzdoRequest<Tags> Tags(string project, string id) =>
        new AzdoRequest<Tags>($"{project}/_apis/build/builds/{id}/tags", 
            new Dictionary<string, object>
            {
                {"api-version", "6.0"}
            });

    public static IAzdoRequest<Tags> Tag(string project, string id, string tag) =>
        new AzdoRequest<Tags>($"{project}/_apis/build/builds/{id}/tags/{tag}",
            new Dictionary<string, object>
            {
                {"api-version", "6.0"}
            });

    public static IAzdoRequest GetLogs1(string project, int buildId) =>
        new AzdoRequest($"{project}/_apis/build/builds/{buildId}/logs/1",
            new Dictionary<string, object>
            {
                {"api-version", "6.0"}
            });

    public static IAzdoRequest GetLogs(string project, string buildId, int logId) =>
        new AzdoRequest($"{project}/_apis/build/builds/{buildId}/logs/{logId}",
            new Dictionary<string, object>
            {
                {"api-version", "7.0"}
            });
}