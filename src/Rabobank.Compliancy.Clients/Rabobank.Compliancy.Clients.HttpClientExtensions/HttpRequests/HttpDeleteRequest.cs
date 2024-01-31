using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection.Metadata;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
///     Provides a base for http DELETE requests.
///     This version supports responses.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
public abstract class HttpDeleteRequest<THandler, TResponse> : HttpRequestBase<THandler, TResponse> where THandler : IHttpClientCallHandler
{
    protected HttpDeleteRequest(THandler callHandler) : base(callHandler)
    {
    }

    protected async override Task<TResponse?> ExecuteHttpRequest(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default,
        CancellationToken cancellationToken = default)
    {
        return await _handler.HandleDeleteCallAsync<TResponse>(uri, authenticationHeaderValue, cancellationToken);
    }
}

/// <summary>
///     Provides a base for http DELETE requests.
///     This version does NOT support responses.
/// </summary>
/// <typeparam name="THandler">The handler type to process the request.</typeparam>
public abstract class HttpDeleteRequest<THandler> : HttpRequestBase<THandler>
    where THandler : IHttpClientCallHandler
{
    protected HttpDeleteRequest(THandler callHandler) : base(callHandler)
    {
    }

    protected async override Task ExecuteHttpRequest(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default,
        CancellationToken cancellationToken = default)
    {
        await _handler.HandleDeleteCallAsync(uri, authenticationHeaderValue, cancellationToken);
    }
}