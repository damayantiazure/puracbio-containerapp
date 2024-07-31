#nullable enable

using FluentValidation;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using Microsoft.FeatureManagement;
using Rabobank.Compliancy.Application;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Security;
using Rabobank.Compliancy.Infra.Sm9Client;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infrastructure;
using Rabobank.Compliancy.Infrastructure.Extensions;
using System;
using System.IO;
using System.Net.Http;
using IAuthorizationService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.IAuthorizationService;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online;

public class Startup : FunctionsStartup
{
    private const string _appInsightsInstrumentationKey = "APPINSIGHTS_INSTRUMENTATIONKEY";

    public override void Configure(IFunctionsHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.AddFilter<ApplicationInsightsLoggerProvider>("", LogLevel.Trace);
            loggingBuilder.AddApplicationInsights(GetEnvironmentVariable(_appInsightsInstrumentationKey));
        });
        builder.Services.AddAzureAppConfiguration();
        builder.Services.AddFeatureManagement();
        RegisterServices(builder.Services, builder.GetContext().Configuration);
    }

    public override void ConfigureAppConfiguration(IFunctionsConfigurationBuilder builder)
    {
        builder.FunctionsConfigurationBuilder(GetEnvironmentVariable("configurationStoreEndpoints"));

        var context = builder.GetContext();
        var environmentName = context.EnvironmentName.ToLower();

        builder.ConfigurationBuilder
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, "logsettings.json"), true, false)
            .AddJsonFile(Path.Combine(context.ApplicationRootPath, $"logsettings.{environmentName}.json"), true, false);
    }

    internal static void RegisterServices(IServiceCollection services, IConfiguration configuration)
    {
        // New way to add all project dependencies at once following clean architecture.
        // The various downstream projects are responsible for managing the collection of their own dependencies to be injected.
        // Should only have a dependency on Application, Infrastructure and Environment Variables.

        services
            .AddFunctionDependencies()
            .AddApplicationDependencies()
            .AddApplicationInfraDependencies(configuration)
            .AddMemoryCache()
            .AddValidatorsFromAssembly(typeof(Startup).Assembly)
            .AddHttpContextAccessor(); 

        // Legacy way to add all dependencies one by one
        var azdoPat = GetEnvironmentVariable("azdoPat");
        services.AddSingleton<IAzdoRestClient>(new AzdoRestClient(azdoPat));

        var extensionSecret = GetEnvironmentVariable("extensionSecret");
        services.AddSingleton<ITokenizer>(new Tokenizer(extensionSecret));
        var extensionName = GetEnvironmentVariable("extensionName");
        var validateGatesHostName = GetEnvironmentVariable("validateGatesHostName");
        services.AddSingleton(new RuleConfig
        {
            ValidateGatesHostName = validateGatesHostName,
        });
        var functionAppHostName = GetEnvironmentVariable("functionAppHostName");
        services.AddSingleton(new Shared.ComplianceConfig
        {
            ExtensionName = extensionName,
            OnlineScannerHostName = functionAppHostName,
        });

        services.AddItsmDependencies();

        var tableConnectionString = GetEnvironmentVariable("tableStorageConnectionString");
        var cloudTableClient = CloudStorageAccount.Parse(tableConnectionString).CreateCloudTableClient();
        services.AddSingleton(cloudTableClient);

        var queueConnectionString = GetEnvironmentVariable("eventQueueStorageConnectionString");
        var storageQueue = Microsoft.Azure.Storage.CloudStorageAccount.Parse(queueConnectionString);
        services.AddSingleton(storageQueue.CreateCloudQueueClient());
        services.AddSingleton(new StorageClientConfig
        {
            EventQueueStorageAccountName = storageQueue.Credentials.AccountName,
            EventQueueStorageAccountKey = Convert.ToBase64String(storageQueue.Credentials.ExportKey())
        });

        services.AddDefaultRules();
        services.AddSingleton(new HttpClient());
        services.AddSingleton<IValidateInputService, ValidateInputService>();
        services.AddSingleton<IPipelineRegistrator, PipelineRegistrator>();
        services.AddSingleton<IPipelineRegistrationRepository>(_ => new PipelineRegistrationRepository(() => cloudTableClient));
        services.AddSingleton<IPipelineRegistrationResolver, PipelineRegistrationResolver>();
        services.AddSingleton<IPipelineRegistrationMapper, PipelineRegistrationMapper>();
        services.AddSingleton<IPipelineRegistrationStorageRepository>(s =>
            new PipelineRegistrationStorageRepository(
                s.GetRequiredService<ILogger<PipelineRegistrationStorageRepository>>(),
                () => cloudTableClient));
        services.AddSingleton<IDeviationStorageRepository, DeviationStorageRepository>();
        services.AddTransient<IAuthorizationService, Shared.Services.AuthorizationService>();
        services.AddScoped<IPipelinesService, Shared.Services.PipelineService>();
        services.AddTransient<IScanCiService, ScanCiService>();
        services.AddTransient<IScanItemsService, ScanItemsService>();
        services.AddTransient<IScanProjectService, ScanProjectService>();
        services.AddTransient<IVerifyComplianceService, VerifyComplianceService>();
        services.AddTransient<ICompliancyReportService, CompliancyReportService>();
        services.AddSingleton<IYamlEnvironmentHelper, YamlEnvironmentHelper>();
        services.AddSingleton<IReleasePipelineService, ReleasePipelineService>();
        services.AddSingleton<IBuildPipelineService, BuildPipelineService>();
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddSingleton<IExclusionStorageRepository, ExclusionStorageRepository>();
        services.AddSingleton<IStorageRepository>(_ => new StorageRepository(() => cloudTableClient));
        services.AddSingleton<IPipelineEvaluatorFactory, PipelineEvaluatorFactory>();
        services.AddSingleton<IYamlHelper, YamlHelper>();
        services.AddSingleton<IManageHooksService, ManageHooksService>();
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}