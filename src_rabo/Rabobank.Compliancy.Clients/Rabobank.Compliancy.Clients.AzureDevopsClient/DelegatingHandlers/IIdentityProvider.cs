using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;

public interface IIdentityProvider
{
    IAuthenticationHeaderContext GetIdentityContext(string identifier);
}