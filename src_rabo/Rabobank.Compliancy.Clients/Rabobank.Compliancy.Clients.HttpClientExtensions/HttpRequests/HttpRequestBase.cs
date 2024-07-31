using Microsoft.AspNetCore.Http.Extensions;
using System.Globalization;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.HttpRequests;

/// <summary>
///     Provides a base for http requests. Requires a <see cref="IHttpClientCallHandler" /> to execute http requests.
///     This version supports responses.
/// </summary>
/// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
/// <typeparam name="THandler">The handler to process the request.</typeparam>
public abstract class HttpRequestBase<THandler, TResponse> :
    HttpRequestBase<THandler> where THandler : IHttpClientCallHandler
{
    private const string _urlUndefinedError = "Url is not defined for {0}, execution cannot continue.";

    protected HttpRequestBase(THandler handler) : base(handler)
    {

    }
    /// <summary>
    ///     Executes the http request defined by inheriting classes using the <see cref="IHttpClientCallHandler" />.
    ///     This version does NOT support responses.
    /// </summary>
    /// <param name="authenticationHeaderValue">Contains the value of the authentication header.</param>
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when the URL is not provided by the implementing request class.</exception>
    public new async Task<TResponse?> ExecuteAsync(AuthenticationHeaderValue? authenticationHeaderValue = default,
        CancellationToken cancellationToken = default)
    {
        if (Uri == null)
        {
            throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, _urlUndefinedError,
                typeof(TResponse)));
        }

        return await ExecuteHttpRequest(Uri, authenticationHeaderValue, cancellationToken);
    }

    protected abstract override Task<TResponse?> ExecuteHttpRequest(Uri uri,
        AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);
}

/// <summary>
///     Provides a base for http requests. Requires a <see cref="IHttpClientCallHandler" /> to execute http requests.
/// </summary>
/// <typeparam name="THandler">The handler to process the request.</typeparam>
public abstract class HttpRequestBase<THandler> where THandler : IHttpClientCallHandler
{

    private const string _urlUndefinedError = "Url is not defined, execution cannot continue.";

    protected THandler _handler;

    private Uri? _uri;

    protected HttpRequestBase(THandler handler)
    {
        _handler = handler;
    }

    protected abstract string? Url { get; }
    protected abstract Dictionary<string, string> QueryStringParameters { get; }

    protected Uri? Uri
    {
        get
        {
            if (_uri != null)
            {
                return _uri;
            }

            if (string.IsNullOrEmpty(Url))
            {
                return null;
            }

            var parsedUrl = Url;
            if (QueryStringParameters.Count > 0)
            {
                parsedUrl += new QueryBuilder(QueryStringParameters);
            }

            _uri = new Uri(parsedUrl, UriKind.Relative);
            return _uri;
        }
    }

    /// <summary>
    ///     Executes the http request defined by inheriting classes using the <see cref="IHttpClientCallHandler" />.
    /// </summary>
    /// <param name="authenticationHeaderValue">Contains the value of the authentication header.</param>
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throws when the URL is not provided by the implementing request class.</exception>
    public async Task ExecuteAsync(AuthenticationHeaderValue? authenticationHeaderValue = default,
        CancellationToken cancellationToken = default)
    {
        if (Uri == null)
        {
            throw new InvalidOperationException(_urlUndefinedError);
        }

        await ExecuteHttpRequest(Uri, authenticationHeaderValue, cancellationToken);
    }

    protected abstract Task ExecuteHttpRequest(Uri uri,
        AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);
}