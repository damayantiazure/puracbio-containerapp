using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using System;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public class YamlReleaseDeploymentEventParser : IYamlReleaseDeploymentEventParser
{
    private const int OrganizationUrlSegment = 1;
    private const int ProjectIdUrlSegment = 2;

    public YamlReleaseDeploymentEvent Parse(string json) =>
        string.IsNullOrWhiteSpace(json) ? null : Parse(JObject.Parse(json));

    private static YamlReleaseDeploymentEvent Parse(JToken jObject)
    {
        if (jObject == null)
        {
            return null;
        }

        return new YamlReleaseDeploymentEvent
        {
            Organization = GetUrlSegment((string)jObject
                .SelectToken("resource.stage._links.web.href"), OrganizationUrlSegment),
            ProjectId = GetUrlSegment((string)jObject
                .SelectToken("resource.stage._links.web.href"), ProjectIdUrlSegment),
            PipelineName = (string)jObject
                .SelectToken("resource.pipeline.name"),
            PipelineId = (string)jObject
                .SelectToken("resource.pipeline.id"),
            StageName = (string)jObject
                .SelectToken("resource.stage.name"),
            StageId = (string)jObject
                .SelectToken("resource.stage.id"),
            RunName = (string)jObject
                .SelectToken("resource.run.name"),
            RunId = (string)jObject
                .SelectToken("resource.run.id"),
            RunUrl = (string)jObject
                .SelectToken("resource.stage._links.web.href"),
            DeploymentStatus = (string)jObject
                .SelectToken("resource.stage.result"),
            CreatedDate = GetDate(jObject["createdDate"])
        };
    }

    private static DateTime GetDate(JToken token) =>
        token == null ? DateTime.MinValue : (DateTime)token;

    private static string GetUrlSegment(string runUrl, int segment) =>
        new Uri(runUrl).Segments[segment].TrimEnd('/');
}