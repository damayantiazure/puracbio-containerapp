#nullable enable

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.FeatureManagement;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Batch;
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
using PipelineService = Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services.PipelineService;

[assembly: WebJobsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Batch;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

        builder.Services.AddLogging();
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
        var commaSeparatedOrganizations = GetEnvironmentVariable("azdoOrganizations");
        services.AddSingleton(commaSeparatedOrganizations.Split(','));

        var azdoPat = GetEnvironmentVariable("azdoPat");
        services.AddSingleton<IAzdoRestClient>(new AzdoRestClient(azdoPat));

        services.AddMemoryCache();

        var extensionSecret = GetEnvironmentVariable("extensionSecret");
        services.AddSingleton<ITokenizer>(new Tokenizer(extensionSecret));
        var extensionName = GetEnvironmentVariable("extensionName");
        var validateGatesHostName = GetEnvironmentVariable("validateGatesHostName");
        services.AddSingleton(new RuleConfig
        {
            ValidateGatesHostName = validateGatesHostName,
        });
        var onlineScannerHostName = GetEnvironmentVariable("onlineScannerHostName");
        services.AddSingleton(new Shared.ComplianceConfig
        {
            ExtensionName = extensionName,
            OnlineScannerHostName = onlineScannerHostName
        });

        services.AddItsmDependencies();

        var connectionString = GetEnvironmentVariable("tableStorageConnectionString");
        var cloudTableClient = CloudStorageAccount.Parse(connectionString).CreateCloudTableClient();
        services.AddSingleton(cloudTableClient);

        services.AddDefaultRules();
        services.AddSingleton(new HttpClient());

        services.AddSingleton<IPipelineRegistrationRepository>(_ => new PipelineRegistrationRepository(() => cloudTableClient));
        services.AddSingleton<IPipelineRegistrationResolver, PipelineRegistrationResolver>();
        services.AddTransient<IPipelineRegistrationMapper, PipelineRegistrationMapper>();
        services.AddSingleton<IPipelineRegistrationStorageRepository>(s =>
            new PipelineRegistrationStorageRepository(
                s.GetRequiredService<ILogger<PipelineRegistrationStorageRepository>>(),
                () => cloudTableClient));
        services.AddSingleton<IDeviationStorageRepository, DeviationStorageRepository>();
        services.AddScoped<IPipelinesService, PipelineService>();
        services.AddTransient<IScanCiService, ScanCiService>();
        services.AddTransient<IScanItemsService, ScanItemsService>();
        services.AddTransient<IScanProjectService, ScanProjectService>();
        services.AddTransient<IVerifyComplianceService, VerifyComplianceService>();
        services.AddSingleton<IYamlEnvironmentHelper, YamlEnvironmentHelper>();
        services.AddSingleton<IReleasePipelineService, ReleasePipelineService>();
        services.AddSingleton<IBuildPipelineService, BuildPipelineService>();
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddSingleton<IPipelineEvaluatorFactory, PipelineEvaluatorFactory>();
        services.AddSingleton<IYamlHelper, YamlHelper>();
        services.AddScanningInfraDependencies(configuration);
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}