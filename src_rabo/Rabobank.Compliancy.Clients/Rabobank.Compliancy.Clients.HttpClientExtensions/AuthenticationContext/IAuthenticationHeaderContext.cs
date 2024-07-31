using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

/// <summary>
/// Interface for specifying the <see cref="AuthenticationHeaderValue"/> operations.
/// </summary>
public interface IAuthenticationHeaderContext
{
    /// <summary>
    /// Gets an <see cref="AuthenticationHeaderValue"/> which can be used to set the authentication header of a <see cref="HttpClient"/>.
    /// </summary>
    /// <returns>Returns <see cref="AuthenticationHeaderValue"/> class.</returns>
    Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// The identifier that holds either a managed identity object id or a personal access token.
    /// </summary>
    string Identifier { get; }
}