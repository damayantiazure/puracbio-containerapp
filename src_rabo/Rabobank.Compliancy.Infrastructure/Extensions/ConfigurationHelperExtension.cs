#nullable enable

using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;

namespace Rabobank.Compliancy.Infrastructure.Extensions;

public static class ConfigurationHelperExtension
{

    public static void FunctionsConfigurationBuilder(this IFunctionsConfigurationBuilder builder, string appConfigurationEndpoints)
    {
        var separator = ';';
        builder = builder ?? throw new ArgumentNullException(nameof(builder));
        builder.ConfigurationBuilder.Build();

        if (!string.IsNullOrEmpty(appConfigurationEndpoints))
        {
            var endpoints = appConfigurationEndpoints.Split(separator);

            foreach (var endpoint in endpoints)
            {
                builder
                    .ConfigurationBuilder
                    .AddAzureAppConfiguration(options =>
                        options.Connect(new Uri(endpoint.Trim()), new DefaultAzureCredential())
                            .UseFeatureFlags())
                    .Build();
            }
        }
    }

    public static bool IsFeatureEnabled(this IConfiguration configuration, string featureName)
    {
        bool isFeatureEnabled = configuration.GetValue<bool>($"FeatureManagement:{featureName}");
        return isFeatureEnabled;
    }
}