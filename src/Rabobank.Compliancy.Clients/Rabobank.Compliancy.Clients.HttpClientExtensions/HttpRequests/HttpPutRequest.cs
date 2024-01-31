using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
/// Provides a base for http PUT requests. When inherited from, the constructor will force the value to be provided.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
/// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
/// <typeparam name="THandler"></typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters",
    Justification = "Acceptable for the use case, since TResponse and TValue belong to the base class")]
public abstract class HttpPutRequest<THandler, TResponse, TValue> : HttpValueRequest<THandler, TResponse, TValue> where THandler : IHttpClientCallHandler
{
    protected HttpPutRequest(TValue value, THandler callHandler) : base(value, callHandler)
    {
        _handler = callHandler;
    }

    protected override async Task<TResponse?> ExecuteHttpRequest(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default)
    {
        return await _handler.HandlePutCallAsync<TResponse, TValue>(uri, Value, authenticationHeaderValue, cancellationToken);
    }
}