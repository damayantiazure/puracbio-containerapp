using Microsoft.Extensions.DependencyInjection;
using Polly;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb;
using System.Net;

namespace Rabobank.Compliancy.Infra.Sm9Client;

public static class DependencyInjection
{
    private const int BackOffInMilliseconds = 300;
    private const int ExponentialBackOffBase = 2;
    private const int RetryCount = 10;
    private const int HttpTimeoutInSeconds = 60;

    public static IServiceCollection AddItsmDependencies(this IServiceCollection services) =>
        services
            .AddConfiguration()
            .AddItsmOauthMessageHandler()
            .AddHttpChangeClient()
            .AddHttpCmdbClient()
            .AddSingleton<IChangeClient, ChangeClient>()
            .AddSingleton<ICmdbClient, CmdbClient>();

    private static IServiceCollection AddConfiguration(this IServiceCollection services) =>
        services.AddSingleton(new ItsmClientConfig(
            GetEnvironmentVariable("itsmEndpointKong"),
            GetEnvironmentVariable("itsmApiResourceKong"),
            GetEnvironmentVariable("globalManagedIdentityClientId")));

    private static IServiceCollection AddItsmOauthMessageHandler(this IServiceCollection serviceCollection) =>
        serviceCollection.AddScoped(serviceProvider =>
        {
            var config = serviceProvider.GetRequiredService<ItsmClientConfig>();
            return new ItsmAzureOathMessageHandler(config);
        });

    private static IServiceCollection AddHttpChangeClient(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(ChangeClient))
            .AddHttpMessageHandler<ItsmAzureOathMessageHandler>()
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<ItsmClientConfig>();
                client.BaseAddress = new Uri(config.Endpoint);
            });

        return services;
    }

    private static IServiceCollection AddHttpCmdbClient(this IServiceCollection services)
    {
        services.AddHttpClient(nameof(CmdbClient))
            .AddHttpMessageHandler<ItsmAzureOathMessageHandler>()
            .AddTransientHttpErrorPolicy(builder => builder
                .OrResult(x => x.StatusCode == HttpStatusCode.InternalServerError)
                .OrResult(x => x.StatusCode == HttpStatusCode.BadGateway)
                .OrResult(x => x.StatusCode == HttpStatusCode.GatewayTimeout)
                .WaitAndRetryAsync(RetryCount,
                    provider => TimeSpan.FromMilliseconds(
                        Math.Pow(ExponentialBackOffBase, provider) * BackOffInMilliseconds)))
            .ConfigureHttpClient((serviceProvider, client) =>
            {
                var config = serviceProvider.GetRequiredService<ItsmClientConfig>();

                client.BaseAddress = new Uri(config.Endpoint);
                client.Timeout = TimeSpan.FromSeconds(HttpTimeoutInSeconds);
            });

        return services;
    }


    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}