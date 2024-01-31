using Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.CallHandlers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.Tests.HttpRequests;

public static class HttpRequestsExtensions
{
    public static string GetProtectedUrl(this HttpRequestBase<ITestHttpClientCallHandler, object> request) =>
        (string)GetPropValue(request, typeof(HttpRequestBase<ITestHttpClientCallHandler, object>), "Url");

    public static string GetProtectedCustomBaseUrl(this HttpRequestBase<ITestHttpClientCallHandler, object> request) =>
        (string)GetPropValue(request, typeof(HttpRequestBase<ITestHttpClientCallHandler, object>), "CustomBaseUrl");

    public static string GetProtectedQueryStringAsString(this HttpRequestBase<ITestHttpClientCallHandler, object> request)
    {
        var queryString = (Dictionary<string, string>)GetPropValue(request, typeof(HttpRequestBase<ITestHttpClientCallHandler, object>), "QueryStringParameters");
        return "?" + string.Join("&", queryString.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));
    }
    public static string GetProtectedValueFromPostRequestAsString(this HttpPostRequest<ITestHttpClientCallHandler, object, object> request) =>
        (string)GetPropValue(request, typeof(HttpPostRequest<ITestHttpClientCallHandler, object, object>), "Value");

#pragma warning disable CS8602 // Dereference of a possibly null reference.
#pragma warning disable CS8603 // Possible null reference return.
    private static object GetPropValue(object? src, Type type, string propName) =>
        type.
            GetProperty(propName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).
            GetValue(src, null);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8602 // Dereference of a possibly null reference.
}