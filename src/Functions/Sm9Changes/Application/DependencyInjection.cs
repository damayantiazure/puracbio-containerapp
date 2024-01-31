using Microsoft.Extensions.DependencyInjection;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSm9ChangesApplicationDependencies(this IServiceCollection services)
    {
        services.AddScoped<ICloseChangeProcess, CloseChangeProcess>();
        return services;
    }
}