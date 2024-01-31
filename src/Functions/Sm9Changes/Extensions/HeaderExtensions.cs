using System.Linq;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Extensions;

public static class HeaderExtensions
{
    public static string BuildId(this HttpRequestHeaders headers) => GetHeaderValue(headers, "BuildId");
    public static string ReleaseId(this HttpRequestHeaders headers) => GetHeaderValue(headers, "ReleaseId");
    public static string ProjectId(this HttpRequestHeaders headers) => GetHeaderValue(headers, "ProjectId");
    public static string PlanUrl(this HttpRequestHeaders headers) => GetHeaderValue(headers, "PlanUrl");

    private static string GetHeaderValue(HttpRequestHeaders headers, string key)
    {
        headers.TryGetValues(key, out var headerValue);
        return headerValue?.FirstOrDefault();
    }
}