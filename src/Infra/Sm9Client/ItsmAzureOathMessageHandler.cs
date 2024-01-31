using Azure.Core;
using Azure.Identity;
using System.Net.Http.Headers;

namespace Rabobank.Compliancy.Infra.Sm9Client;
public class ItsmAzureOathMessageHandler : DelegatingHandler
{
    private readonly TokenRequestContext _tokenRequestContext;
    private readonly DefaultAzureCredential _credentials;

    public ItsmAzureOathMessageHandler(ItsmClientConfig itsmClientConfig)
    {
        _tokenRequestContext = new TokenRequestContext(new[] { itsmClientConfig.Resource });
        _credentials = new DefaultAzureCredential(new DefaultAzureCredentialOptions
        {
            ManagedIdentityClientId = itsmClientConfig.ManagedIdentityClientId
        });
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tokenResult = await _credentials.GetTokenAsync(_tokenRequestContext, cancellationToken);
        var authorizationHeader = new AuthenticationHeaderValue("Bearer", tokenResult.Token);
        request.Headers.Authorization = authorizationHeader;
        return await base.SendAsync(request, cancellationToken);
    }
}
