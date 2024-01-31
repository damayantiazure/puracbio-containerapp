#nullable enable

namespace Rabobank.Compliancy.Domain.Extensions;

public static class RequestMessageExtensions
{
    private static readonly string[] _sensitiveHeaders = { "x-functions-key" };

    public static HttpRequestMessage? StripSensitiveInformationFromHeader(this HttpRequestMessage? request)
    {
        if (request == null)
        {
            return null;
        }

        // Strip authorization header
        request.Headers.Authorization = null;

        // Strip other sensitive headers
        foreach (var header in _sensitiveHeaders)
        {
            request.Headers.Remove(header);
        }

        return request;
    }
}
