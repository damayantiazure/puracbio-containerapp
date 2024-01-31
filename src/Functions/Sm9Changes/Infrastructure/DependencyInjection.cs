using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSm9ChangesInfraDependencies(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IChangeService, ServiceManagerChangeService>();
        return serviceCollection;
    }
}