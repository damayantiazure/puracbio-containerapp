using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Clients.AzureDevopsClient;

namespace Rabobank.Compliancy.Tests.Helpers;

public static class ServiceProviderHelper
{
    public static IServiceProvider InitAzureDevopsClient()
    {
        var services = new ServiceCollection();
        return InitAzureDevopsClient(services);
    }

    public static IServiceProvider InitAzureDevopsClient(IServiceCollection services) =>
        services
            .AddAzdoClientDependencies()
            .BuildServiceProvider();

    public static TService GetServiceOrThrow<TService>(this IServiceProvider serviceProvider)
    {
        var service = serviceProvider.GetService<TService>();

        if (service == null)
        {
            throw new InvalidOperationException("Requested service has not been added to the serviceprovider, type: " + typeof(TService));
        }

        return service;
    }
}