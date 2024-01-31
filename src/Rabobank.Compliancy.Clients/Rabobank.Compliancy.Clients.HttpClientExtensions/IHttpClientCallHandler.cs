using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions;

/// <summary>
/// Provides a method for each type of http request and handles it asynchronously. Uses a semaphore, initialized at startup, to prevent 429's or general overload problems.
/// </summary>
public interface IHttpClientCallHandler
{
    string Identifier { get; }

    /// <summary>
    /// Handles a DELETE call asynchronously, do not process any results.
    /// </summary>
    /// <param name="uri">The URL the request is sent to.</param>
    /// <param name="authenticationHeaderValue">Optional parameter that contains information to customize the authorization header.</param>
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task HandleDeleteCallAsync(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a DELETE call asynchronously, mapping the result to <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
    /// <param name="uri">The URL the request is sent to.</param>    
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task<TResponse?> HandleDeleteCallAsync<TResponse>(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a GET call asynchronously, mapping the result to <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
    /// <param name="uri">The URL the request is sent to.</param>    
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task<TResponse?> HandleGetCallAsync<TResponse>(Uri uri, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a POST call asynchronously, mapping the result to <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
    /// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
    /// <param name="uri">The URL the request is sent to.</param>
    /// <param name="value">Contains the information that is posted as the HTTP request body.</param>    
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task<TResponse?> HandlePostCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a PUT call asynchronously, mapping the result to <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
    /// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
    /// <param name="uri">The URL the request is sent to.</param>
    /// <param name="value">Contains the information that is posted as the HTTP request body.</param>    
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task<TResponse?> HandlePutCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a PATCH call asynchronously, mapping the result to <typeparamref name="TResponse"/>
    /// </summary>
    /// <typeparam name="TResponse">The type the result of the http request is mapped on.</typeparam>
    /// <typeparam name="TValue">The type of the value that represents the body of the request.</typeparam>
    /// <param name="uri">The URL the request is sent to.</param>
    /// <param name="value">Contains the information that is posted as the HTTP request body.</param>    
    /// <param name="cancellationToken">Cancels the http request if necessary.</param>
    /// <returns></returns>
    Task<TResponse?> HandlePatchCallAsync<TResponse, TValue>(Uri uri, TValue value, AuthenticationHeaderValue? authenticationHeaderValue = default, CancellationToken cancellationToken = default);
}