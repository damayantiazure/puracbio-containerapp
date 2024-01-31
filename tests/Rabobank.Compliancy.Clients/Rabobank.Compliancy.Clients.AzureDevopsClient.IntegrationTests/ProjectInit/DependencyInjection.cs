using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit.Interfaces;
using Rabobank.Compliancy.Tests.Helpers;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.IntegrationTests.ProjectInit;

public static class DependencyInjection
{
    public static ServiceCollection AddProjectInitDependencies(this ServiceCollection services)
    {
        var token = Environment.GetEnvironmentVariable("azdoPat") ?? throw new InvalidOperationException("azdoPat must be present in configuration.");

        services.AddSingleton<IProjectInitHttpClientCallHandler>(serviceProvider => new ProjectInitHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));

        services.AddHttpClient(ProjectInitHttpClientCallHandler.SpecificBaseUrl,
            client =>
            {
                client.BaseAddress = new Uri(ProjectInitHttpClientCallHandler.SpecificBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(300);
                client.DefaultRequestHeaders.Authorization = token.ToBasicAuthenticationHeader();
            }
        ).AddTransientHttpErrorPolicy(policyBuilder =>
            policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5))
        );

        services.AddScoped<IProjectInitRepository, ProjectInitRepository>();

        return services;
    }
}