using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using System;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public class ClassicReleaseDeploymentEventParser : IClassicReleaseDeploymentEventParser
{
    private const int OrganizationUrlSegment = 1;
    private const int ProjectUrlSegment = 2;

    public ClassicReleaseDeploymentEvent Parse(string json) =>
        string.IsNullOrEmpty(json) ? null : Parse(JObject.Parse(json));

    private static ClassicReleaseDeploymentEvent Parse(JObject jObject)
    {
        if (jObject == null || IsTestEvent(jObject))
        {
            return null;
        }

        return new ClassicReleaseDeploymentEvent
        {
            Organization = GetUrlSegment((string)jObject
                    .SelectToken("resource.url"),
                OrganizationUrlSegment),
            ProjectName = Flurl.Url.Decode(GetUrlSegment((string)jObject
                    .SelectToken("resource.url"),
                ProjectUrlSegment), true),
            ProjectId = (string)jObject
                .SelectToken("resourceContainers.project.id"),
            ReleaseId = (string)jObject
                .SelectToken("resource.id"),
            ReleaseUrl = (string)jObject
                .SelectToken("resource.url"),
            StageName = (string)jObject
                .SelectToken("resource.stageName"),
            CreatedDate = GetDate(jObject["createdDate"]),
        };
    }

    private static bool IsTestEvent(JObject jObject) =>
        jObject.SelectToken("message.markdown").ToString().Contains("http://fabfiber.visualstudio.com");

    private static DateTime GetDate(JToken token) =>
        token == null ? DateTime.MinValue : (DateTime)token;

    private static string GetUrlSegment(string runUrl, int segment) =>
        new Uri(runUrl).Segments[segment].TrimEnd('/');
}