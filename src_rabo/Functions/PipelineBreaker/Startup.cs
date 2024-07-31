#nullable enable

using System;
using System.IO;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Functions.PipelineBreaker;
using Rabobank.Compliancy.Functions.PipelineBreaker.Model;
using Rabobank.Compliancy.Functions.PipelineBreaker.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Security;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infrastructure;
using AuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.AuthorizationService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.PipelineBreaker;

public class Startup : FunctionsStartup
{
    private ILogger _logger = null!;

    public override void Configure(IFunctionsHostBuilder builder)
    {
        using var loggerFactory = LoggerFactory.Create(loggingBuilder =>
        {
            loggingBuilder.SetMinimumLevel(LogLevel.Debug);
            loggingBuilder.AddEventLog();
        });

        try
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.Services.AddLogging();

            _logger = loggerFactory.CreateLogger(nameof(Startup));
            _logger.LogInformation("Got here in Startup");

            RegisterServices(builder.Services, builder.GetContext().Configuration);
            _logger.LogInformation("After register services");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Exception occurred in startup: {ex}");
            _logger.LogInformation($"Info: Exception occurred in startup: {ex}");
        }
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

        var blockingEnabled = false;
        if (bool.TryParse(GetEnvironmentVariable("blockUnregisteredPipelinesEnabled"), out var enabled))
        {
            blockingEnabled = enabled;
        }

        var blockingIncompliantEnabled = false;
        if (bool.TryParse(GetEnvironmentVariable("blockIncompliantPipelinesEnabled"), out var enabledBlocking))
        {
            blockingIncompliantEnabled = enabledBlocking;
        }

        var throwWarnings = false;
        if (bool.TryParse(GetEnvironmentVariable("throwWarningsIncompliantPipelinesEnabled"), out var warningsEnabled))
        {
            throwWarnings = warningsEnabled;
        }

        var extensionName = GetEnvironmentVariable("extensionName");
        var validateGatesHostName = GetEnvironmentVariable("validateGatesHostName");
        services.AddSingleton(new RuleConfig
        {
            ValidateGatesHostName = validateGatesHostName
        });

        services.AddMemoryCache();
        services.AddDefaultRules();

        services.AddSingleton(new PipelineBreakerConfig
        {
            BlockUnregisteredPipelinesEnabled = blockingEnabled,
            BlockIncompliantPipelinesEnabled = blockingIncompliantEnabled,
            ThrowWarningsIncompliantPipelinesEnabled = throwWarnings
        });

        services.AddSingleton(new ComplianceConfig { ExtensionName = extensionName });
        services.AddSingleton<IValidateInputService, ValidateInputService>();

        var connectionString = GetEnvironmentVariable("tableStorageConnectionString");
        var cloudTableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();

        services.AddSingleton<IPipelineRegistrationRepository>(_ => 
            new PipelineRegistrationRepository(() => cloudTableClient));
        services.AddSingleton<IDeviationStorageRepository>(_ => 
            new DeviationStorageRepository(() => cloudTableClient));
        services.AddSingleton<IStorageRepository>(_ => new StorageRepository(() => cloudTableClient));

        services.AddSingleton<IExclusionStorageRepository>(serviceProvider => new ExclusionStorageRepository(
            serviceProvider.GetRequiredService<IStorageRepository>(),
            serviceProvider.GetRequiredService<IAuthorizationService>()));

        services.AddSingleton<IYamlEnvironmentHelper, YamlEnvironmentHelper>();
        services.AddSingleton<IPipelineRegistrationResolver, PipelineRegistrationResolver>();
        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<IBuildPipelineService, BuildPipelineService>();
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddSingleton<IReleasePipelineService, ReleasePipelineService>();
        services.AddScoped<IPipelineBreakerService, PipelineBreakerService>();

        var extensionSecret = GetEnvironmentVariable("extensionSecret");
        services.AddSingleton<ITokenizer>(new Tokenizer(extensionSecret));
        services.AddSingleton<IPipelineEvaluatorFactory, PipelineEvaluatorFactory>();
        services.AddSingleton<IYamlHelper, YamlHelper>();

        services.AddClientInfraDependencies(configuration);
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}