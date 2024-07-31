namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
/// Provides a base for value http requests. These are POST, PUT and PATCH. Defines the Value property to be re-used by the implementing classes. When inherited from, the constructor will force the value to be provided.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
/// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
/// <typeparam name="THandler">The type of the handler.</typeparam>
[System.Diagnostics.CodeAnalysis.SuppressMessage("Major Code Smell", "S2436:Types and methods should not have too many generic parameters",
    Justification = "Acceptable for the use case, since TResponse and TValue belong to the base class")]
public abstract class HttpValueRequest<THandler, TResponse, TValue> : HttpRequestBase<THandler, TResponse> where THandler : IHttpClientCallHandler
{
    public TValue Value { get; set; }

    protected HttpValueRequest(TValue value, THandler handler) : base(handler)
    {
        Value = value;
    }
}