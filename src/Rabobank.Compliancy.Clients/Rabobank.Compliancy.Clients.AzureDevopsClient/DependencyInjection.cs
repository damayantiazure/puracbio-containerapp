using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.Services.Client;
using Microsoft.VisualStudio.Services.Common;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Rabobank.Compliancy.Clients.AzureDevopsClient.DelegatingHandlers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Helpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.HttpClientCallHandlers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Clients.AzureDevopsClient.RateLimitControl;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Clients.HttpClientExtensions;
using Rabobank.Compliancy.Clients.HttpClientExtensions.AuthenticationContext;
using System.Globalization;
using System.Security;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient;

// This Extension Class is responsible for the dependency injection of everything a project using this project needs
public static class DependencyInjection
{
    // Key's for configuration items.
    private const string Identities = nameof(Identities);

    private const string AzdoPat = nameof(AzdoPat);

    private const string _invalidConfigurationError = "Please provide a valid value for environment variable '{0}'";

    public static IServiceCollection AddAzdoClientDependencies(this IServiceCollection collection) =>
        collection
            .RegisterHttpClientsAndHandlers()
            .RegisterRepositories()
            .RegisterHelpers();

    /// <summary>
    /// HttpClients should be added as singletons to prevent socket exhaustion (repeatedly instantiating
    /// an HTTP Client will cause this).
    /// IHttpClientFactory is used to prevent problems with expired DNS records (A singleton HTTP client
    /// will not re-check the base URLs DNS unless the application is restarted).
    /// The Factory is added as a singleton by services.AddHttpClient(string name) and will be asked
    /// for a (lightweight) HTTP Client with every HTTP request we do. This is the recommended way,
    /// because the HTTPClientFactory re-uses the message handler (no socket exhaustion, fast
    /// initialization), pools the DNS time (no DNS errors on long-running applications) and allows for
    /// easy to use polly extensions packages (see below). HTTP Clients can now be used and disposed as
    /// pleased by injecting IHttpClientFactory and calling CreateClient(string name) on it. In our case
    /// the name is our token, so we have a unique client for every token.
    /// </summary>
    /// <param name="services"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private static IServiceCollection RegisterHttpClientsAndHandlers(this IServiceCollection services)
    {
        var identityHeaderContexts = GetAuthenticationHeaderContexts();
        if (!identityHeaderContexts.Any())
        {
            throw new InvalidOperationException($"{nameof(Identities)} or {nameof(AzdoPat)} not found in configuration");
        }
        var clientIdentities = identityHeaderContexts.Select(i => i.Identifier).ToArray();

        // Get client identities from configuration. This can be a comma separated list of PATs or managed identities
        services.AddSingleton<IAzdoRateLimitObserver>(serviceProvider =>
            new AzdoRateLimitObserver(clientIdentities, serviceProvider.GetRequiredService<ILogger<AzdoRateLimitObserver>>()));

        var identityProvider = new IdentityProvider(identityHeaderContexts);

        services.AddSingleton<IIdentityProvider>(identityProvider);
        var baseUrls = GetAllBaseUrlsFromHandlers();

        // Create HttpClients based on the different baseUrls
        foreach (var baseUrl in baseUrls)
        {
            services.AddHttpClient(baseUrl, client =>
            {
                client.BaseAddress = new Uri(baseUrl);
            })
            .AddHttpMessageHandler((provider) =>
            {
                return new AuthenticationDelegateHandler(provider.GetRequiredService<IAzdoRateLimitObserver>(), identityProvider);
            })
            .AddTransientHttpErrorPolicy(policyBuilder =>
                policyBuilder.WaitAndRetryAsync(Backoff.DecorrelatedJitterBackoffV2(TimeSpan.FromSeconds(1), 5))
            );
        }

        // Register call handlers
        services.AddSingleton<IDevHttpClientCallHandler>(serviceProvider => new DevHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<IAuditserviceHttpClientCallHandler>(serviceProvider => new AuditserviceHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<IExtmgmtHttpClientCallHandler>(serviceProvider => new ExtmgmtHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<IVsaexHttpClientCallHandler>(serviceProvider => new VsaexHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<IVsrmHttpClientCallHandler>(serviceProvider => new VsrmHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));
        services.AddSingleton<IVsspsHttpClientCallHandler>(serviceProvider => new VsspsHttpClientCallHandler(serviceProvider.GetRequiredService<IHttpClientFactory>()));

        return services;
    }

    private static IServiceCollection RegisterRepositories(this IServiceCollection services) =>
        services
            .AddScoped<IAccessControlListsRepository, AccessControlListsRepository>()
            .AddScoped<IApplicationGroupRepository, ApplicationGroupRepository>()
            .AddScoped<IApplicationGroupRepository, ApplicationGroupRepository>()
            .AddScoped<IAuthorizationRepository, AuthorizationRepository>()
            .AddScoped<IBuildRepository, BuildRepository>()
            .AddScoped<ICheckConfigurationRepository, CheckConfigurationRepository>()
            .AddScoped<IDistributedTaskRepository, DistributedTaskRepository>()
            .AddScoped<IEnvironmentRepository, EnvironmentRepository>()
            .AddScoped<IExtensionDataRepository, ExtensionDataRepository>()
            .AddScoped<IGitRepositoryRepository, GitRepositoryRepository>()
            .AddScoped<IHooksRepository, HooksRepository>()
            .AddScoped<IIdentityRepository, IdentityRepository>()
            .AddScoped<IOperationRepository, OperationsRepository>()
            .AddScoped<IPermissionRepository, PermissionRepository>()
            .AddScoped<IPipelineRepository, PipelineRepository>()
            .AddScoped<IProjectRepository, ProjectRepository>()
            .AddScoped<IReleaseRepository, ReleaseRepository>()
            .AddScoped<ITaskGroupRepository, TaskGroupRepository>()
            .AddScoped<IWorkItemTrackingRepository, WorkItemTrackingRepository>();

    private static IServiceCollection RegisterHelpers(this IServiceCollection services)
    {
        services
            .AddScoped<IRecursiveIdentityCacheBuilder, RecursiveIdentityCacheBuilder>();

        return services;
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

    private static IEnumerable<IAuthenticationHeaderContext> GetAuthenticationHeaderContexts()
    {
        var semicolonSeparatedClientIds = Environment.GetEnvironmentVariable(Identities, EnvironmentVariableTarget.Process);
        if (semicolonSeparatedClientIds != null)
        {
            var managedIdentityClientIds = semicolonSeparatedClientIds.Split(';');
            foreach (var managedIdentityClientId in managedIdentityClientIds)
            {
                yield return CreateManagedIdentityContext(managedIdentityClientId);
            }
        }

        var commaSeparatedPats = Environment.GetEnvironmentVariable(AzdoPat, EnvironmentVariableTarget.Process);
        if (commaSeparatedPats != null)
        {
            var pats = commaSeparatedPats.Split(',');
            foreach (var pat in pats)
            {
                yield return new PersonalAccessTokenContext(pat);
            }
        }
    }

    private static IAuthenticationHeaderContext CreateManagedIdentityContext(string ManagedIdentityClientId)
    {
        var tokenRequestContext = new TokenRequestContext(VssAadSettings.DefaultScopes);
        var credential = new DefaultAzureCredential(
            new DefaultAzureCredentialOptions
            {
                ManagedIdentityClientId = ManagedIdentityClientId
            });
        return new TokenCredentialContext(credential, tokenRequestContext, ManagedIdentityClientId);
    }
}