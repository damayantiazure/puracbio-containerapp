using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
///     Provides a base for http PATCH requests. When inherited from, the constructor will force the value to be provided.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
/// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
/// <typeparam name="THandler">The handler type to process the request.</typeparam>
[SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters",
    Justification = "Acceptable for the use case, since TResponse and TValue belong to the base class")]
public abstract class HttpPatchRequest<THandler, TResponse, TValue> : HttpValueRequest<THandler, TResponse, TValue>
    where THandler : IHttpClientCallHandler
{
    protected HttpPatchRequest(TValue value, THandler callHandler) : base(value, callHandler)
    {
        _handler = callHandler;
    }

    protected async override Task<TResponse?> ExecuteHttpRequest(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        return await _handler.HandlePatchCallAsync<TResponse, TValue>(uri, Value, authenticationHeaderValue, cancellationToken);
    }
}