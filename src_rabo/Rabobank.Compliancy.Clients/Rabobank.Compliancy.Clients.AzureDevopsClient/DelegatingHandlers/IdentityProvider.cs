using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;

public class IdentityProvider : IIdentityProvider
{
    private readonly Dictionary<string, IAuthenticationHeaderContext> _identities = new();

    public IdentityProvider(IEnumerable<IAuthenticationHeaderContext> authenticationHeaderContexts)
    {
        foreach (var context in authenticationHeaderContexts)
        {
            _identities.Add(context.Identifier, context);
        }
    }

    public IAuthenticationHeaderContext GetIdentityContext(string identifier)
    {
        return _identities[identifier];
    }
}