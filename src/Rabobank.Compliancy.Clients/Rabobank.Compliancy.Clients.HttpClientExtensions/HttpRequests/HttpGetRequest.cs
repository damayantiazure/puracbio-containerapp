using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
///     Provides a base for http GET requests.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
/// <typeparam name="THandler">Type of handler to process the request.</typeparam>
public abstract class HttpGetRequest<THandler, TResponse> : HttpRequestBase<THandler, TResponse>
    where THandler : IHttpClientCallHandler
{
    protected HttpGetRequest(THandler callHandler) : base(callHandler)
    {
        _handler = callHandler;
    }

    protected async override Task<TResponse?> ExecuteHttpRequest(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        return await _handler.HandleGetCallAsync<TResponse>(uri, authenticationHeaderValue, cancellationToken);
    }
}