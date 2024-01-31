#nullable enable

using Azure.Identity;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Functions.Sm9Changes;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Infrastructure;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.Sm9Client;
using System;
using Rabobank.Compliancy.Infrastructure;
using System.IO;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.Sm9Changes;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder) => 
        RegisterServices(builder.Services, builder.GetContext().Configuration);

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();
        var environmentName = context.EnvironmentName.ToLower();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"logsettings.json"), true, false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"logsettings.{environmentName}.json"), true, false)
            .AddEnvironmentVariables()
            .AddAzureAppConfiguration(options =>
            {
                var appConfigurationEndpoint = GetEnvironmentVariable("configurationStoreEndpoints");
                if (!string.IsNullOrEmpty(appConfigurationEndpoint))
                {
                    options.Connect(new Uri(appConfigurationEndpoint), new DefaultAzureCredential())
                        .UseFeatureFlags();
                }
            });
    }

    internal static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging();
        services.AddAzureAppConfiguration();
        services.AddFeatureManagement();

        var azdoPat = GetEnvironmentVariable("azdoPat");
        services.AddSingleton<IAzdoRestClient>(new AzdoRestClient(azdoPat));

        services.AddItsmDependencies();

        services.AddTransient<IClassicReleaseApproverService, ClassicReleaseApproverService>();
        services.AddTransient<IPullRequestApproverService, PullRequestApproverService>();
        services.AddTransient<IYamlReleaseApproverService, YamlReleaseApproverService>();
        services.AddTransient<ISm9ChangesService, Sm9ChangesService>();

        services.AddSingleton(new TimeOutSettings { TimeOutValue = 150 });

        services.AddSm9ChangesInfraDependencies();
        services.AddSm9ChangesApplicationDependencies();
        services.AddCommonInfraDependencies(configuration);
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}