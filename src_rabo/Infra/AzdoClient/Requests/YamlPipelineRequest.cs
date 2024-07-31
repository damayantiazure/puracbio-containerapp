using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class YamlPipeline
{
    private const int TimeoutInSeconds = 180;

    public static IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse> Parse(
        string projectId, string pipelineId, string pipelineVersion) =>
        new AzdoRequest<YamlPipelineRequest, YamlPipelineResponse>(
            $"{projectId}/_apis/pipelines/{pipelineId}/preview", new Dictionary<string, object>
            {
                { "pipelineVersion", pipelineVersion },
                { "api-version", "7.1-preview.1" }
            }, TimeoutInSeconds);

    public static IAzdoRequest<YamlPipelineRequest, YamlPipelineResponse> Parse(string project, string pipelineId) =>
        new AzdoRequest<YamlPipelineRequest, YamlPipelineResponse>(
            $"{project}/_apis/pipelines/{pipelineId}/runs", new Dictionary<string, object>
            {
                {"api-version", "6.1-preview.1"}
            }, TimeoutInSeconds);

    public static IAzdoRequest<YamlPipelinesResponse> Parse(string project) =>
        new AzdoRequest<YamlPipelinesResponse>(
            $"{project}/_apis/pipelines", new Dictionary<string, object>
            {
                {"api-version", "5.1-preview"}
            });

    public static IAzdoRequest<YamlPipelineInformationResponse> PipelineInformation(string project, string pipelineId) =>
        new AzdoRequest<YamlPipelineInformationResponse>(
            $"{project}/_apis/pipelines/{pipelineId}", new Dictionary<string, object>
            {
                {"api-version", "6.1-preview.1"}
            }, TimeoutInSeconds);

    public class YamlPipelineRequest
    {
        public bool PreviewRun { get; set; } = true;
    }
}