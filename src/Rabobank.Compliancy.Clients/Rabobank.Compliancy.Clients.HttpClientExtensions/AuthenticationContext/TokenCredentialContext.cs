using Azure.Core;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

/// <inheritdoc />
public class TokenCredentialContext : IAuthenticationHeaderContext
{
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly TokenCredential _tokenCredential;

    public TokenCredentialContext(TokenCredential credential, TokenRequestContext requestcontext, string? uniqueIdentifier = default)
    {
        _tokenCredential = credential;
        _tokenRequestContext = requestcontext;
        Identifier = uniqueIdentifier ?? Guid.NewGuid().ToString();
    }

    /// <inheritdoc />
    public async Task<AuthenticationHeaderValue> GetAuthenticationHeaderAsync(CancellationToken cancellationToken = default)
    {
        var accessToken = await _tokenCredential.GetTokenAsync(_tokenRequestContext, cancellationToken);

        return new AuthenticationHeaderValue("Bearer", accessToken.Token);
    }

    /// <inheritdoc />
    public string Identifier { get; }
}