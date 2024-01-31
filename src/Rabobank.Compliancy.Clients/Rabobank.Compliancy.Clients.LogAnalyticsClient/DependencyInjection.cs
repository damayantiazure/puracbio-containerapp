using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories;
using Rabobank.Compliancy.Clients.LogAnalyticsClient.Repositories.Interfaces;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace Rabobank.Compliancy.Clients.LogAnalyticsClient;

public static class DependencyInjection
{
    private const string environmentVariableNotFoundError = "Please provide a valid value for environment variable {0}";

    public static IServiceCollection AddLogAnalyticsClientDependencies(this IServiceCollection collection)
    {
        var tenantId = GetEnvironmentVariable("tenantId");
        var clientId = GetEnvironmentVariable("clientId");
        var clientSecret = GetEnvironmentVariable("clientSecret");
        var workspaceId = GetEnvironmentVariable("logAnalyticsWorkspace");
        var key = GetEnvironmentVariable("logAnalyticsKey");

        var loganalyticsConfiguraiton = new LogAnalyticsConfiguration(tenantId, clientId, clientSecret, workspaceId, key);
        collection.AddSingleton<ILogAnalyticsConfiguration>(loganalyticsConfiguraiton);

        var logAnalyticsClient = new LogAnalytics.Client.LogAnalyticsClient(loganalyticsConfiguraiton.WorkspaceId, loganalyticsConfiguraiton.Key);
        collection.AddSingleton<LogAnalytics.Client.ILogAnalyticsClient>(logAnalyticsClient);

        collection.AddSingleton<ILogAnalyticsRepository, LogAnalyticsRepository>();

        var baseUrls = GetAllBaseUrlsFromHandlers();
        var hashedToken = Encoding.Default.GetString(SHA256.HashData(Encoding.Default.GetBytes(nameof(ILogAnalyticsCallHandler))));

        collection.AddSingleton<ILogAnalyticsCallHandler>(serviceProvider => new LogAnalyticsCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>(), hashedToken));
        collection.AddSingleton<IMicrosoftOnlineHandler>(serviceProvider => new MicrosoftOnlineHandler(serviceProvider.GetRequiredService<IHttpClientFactory>(), hashedToken));
        foreach (var baseUrl in baseUrls)
        {
            collection.AddHttpClient(hashedToken + baseUrl,
                client =>
                {
                    client.BaseAddress = new Uri(baseUrl);
                }
            ).AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5))
            );
        }

        collection.AddSingleton<IHttpClientCallDistributor<ILogAnalyticsCallHandler>, HttpClientCallDistributor<ILogAnalyticsCallHandler>>();
        collection.AddSingleton<IHttpClientCallDistributor<IMicrosoftOnlineHandler>, HttpClientCallDistributor<IMicrosoftOnlineHandler>>();

        return collection;
    }

    private static ICollection<string> GetAllBaseUrlsFromHandlers()
    {
        var baseUrls = new HashSet<string>();
        var superType = typeof(SpecificHttpClientCallHandlerBase);
        foreach (var type in superType.Assembly.GetTypes().Where(type => superType.IsAssignableFrom(type) && !type.IsAbstract))
        {
            var value = type.GetField("SpecificBaseUrl")?.GetValue(null);
            if (value != null)
            {
                baseUrls.Add((string)value);
            }
        }
        return baseUrls;
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName, string.Format(CultureInfo.InvariantCulture, environmentVariableNotFoundError, variableName));
}