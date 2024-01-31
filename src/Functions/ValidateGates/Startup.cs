#nullable enable

using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Functions.ValidateGates;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infrastructure;
using System;
using System.IO;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.ValidateGates;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddLogging();
        RegisterServices(builder.Services, builder.GetContext().Configuration);
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        var context = builder.GetContext();
        var environmentName = context.EnvironmentName.ToLower();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "logsettings.json"), true, false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"logsettings.{environmentName}.json"), true, false);
    }

    internal static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        var azdoPat = GetEnvironmentVariable("azdoPat");
        services.AddSingleton<IAzdoRestClient>(new AzdoRestClient(azdoPat));
        services.AddTransient<IYamlReleaseApproverService, YamlReleaseApproverService>();
        services.AddTransient<IClassicReleaseApproverService, ClassicReleaseApproverService>();
        services.AddTransient<IPullRequestApproverService, PullRequestApproverService>();
        services.AddSingleton<IValidateInputService>(new ValidateInputService());
        services.AddCommonInfraDependencies(configuration);
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}