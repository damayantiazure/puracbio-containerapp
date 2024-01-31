
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

/// <inheritdoc />
public class PersonalAccessTokenContext : IAuthenticationHeaderContext
{
    private readonly string _personalAccessToken;

    public PersonalAccessTokenContext(string personalAccessToken)
    {
        _personalAccessToken = personalAccessToken;
        // We hash the token to create a unique, reproducable identifier that is not the actual secret
        Identifier = Encoding.Default.GetString(SHA256.HashData(Encoding.Default.GetBytes(personalAccessToken)));
    }

    /// <inheritdoc />
    public Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult<AuthenticationHeaderValue>(new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes($":{_personalAccessToken}"))));
    }

    /// <inheritdoc />
    public string Identifier { get; }
}