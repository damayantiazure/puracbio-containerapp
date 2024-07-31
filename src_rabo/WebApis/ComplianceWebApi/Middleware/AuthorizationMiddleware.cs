using Azure.Core;
using ComplianceWebApi.Configurations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace ComplianceWebApi.Middleware;

public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    private static (string, string) GetAccessTokenFromRequestHeader(HttpRequest request)
    {
        if (request.Headers.TryGetValue("Authorization", out var authInfo) && authInfo.Any())
        {
            var authValue = authInfo.First();
            var authValues = $"{authValue}".Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (authValues != null && authValues.Length > 0)
            {
                var scheme = authValues[0];
                var token = authValues[1];
                return (scheme, token);
            }
        }
        throw new UnauthorizedAccessException("Failed to validate authorization token.");
    }

    public async Task InvokeAsync(
        HttpContext httpContext,
        [FromServices] ILogger<AuthorizationMiddleware> logger,
        [FromServices] AzureDevOpsClientConfig config,
        [FromServices] IAzdoRestClient azdoRestClient)
    {
        try
        {
            var (scheme, token) = GetAccessTokenFromRequestHeader(httpContext.Request);
            var connectionData =
                await azdoRestClient.GetWithTokenAsync(Connections.ConnectionData(), token, config.orgName);

            if (connectionData.AuthenticatedUser.SubjectDescriptor == null)
            {
                throw new UnauthorizedAccessException("Access denied");
            }

            logger.LogInformation($"Connection data: {connectionData.AuthenticatedUser.ProviderDisplayName}");
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogError(ex, "Error in AuthorizationMiddleware");
            throw;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error in AuthorizationMiddleware");
            throw new UnauthorizedAccessException("Access denied", ex);
        }

        await _next(httpContext);
    }
}

public static class AuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseDevOpsAccessTokenValidation(
        this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<AuthorizationMiddleware>();
    }
}
