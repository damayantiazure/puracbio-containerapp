#nullable enable

using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Azure.Storage.Queue;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rabobank.Compliancy.Application;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules;
using Rabobank.Compliancy.Functions.AuditLogging;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Rabobank.Compliancy.Functions.AuditLogging.Services;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Rabobank.Compliancy.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Startup))]

namespace Rabobank.Compliancy.Functions.AuditLogging;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        if (builder == null)
        {
            throw new ArgumentNullException(nameof(builder));
        }

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
        // New way to add all project dependencies at once following clean architecture.
        // The various downstream projects are responsible for managing the collection of their own dependencies to be injected.
        // Should only have a dependency on Application, Infrastructure and Environment Variables.
        services.AddApplicationDependencies()
            .AddApplicationInfraDependencies(configuration)
            .AddMemoryCache();

        var azdoPat = GetEnvironmentVariable("azdoPat");
        services.AddSingleton<IAzdoRestClient>(new AzdoRestClient(azdoPat));

        var commaSeparatedOrganizations = GetEnvironmentVariable("azdoOrganizations");
        services.AddSingleton(commaSeparatedOrganizations.Split(','));

        services.AddTransient<IYamlReleaseApproverService, YamlReleaseApproverService>();
        services.AddTransient<IClassicReleaseApproverService, ClassicReleaseApproverService>();
        services.AddTransient<IPullRequestApproverService, PullRequestApproverService>();

        var tableConnectionString = GetEnvironmentVariable("tableStorageConnectionString");
        var cloudTableClient = CloudStorageAccount.Parse(tableConnectionString).CreateCloudTableClient();
        services.AddSingleton(cloudTableClient);
        var queueConnectionString = GetEnvironmentVariable("eventQueueStorageConnectionString");
        var storageQueue = Microsoft.Azure.Storage.CloudStorageAccount.Parse(queueConnectionString);

        services.AddSingleton(async _ =>
        {
            var cloudQueueClient = storageQueue.CreateCloudQueueClient();
            await CreateQueuesIfNotExists(cloudQueueClient);
            return cloudQueueClient;
        });

        services.AddSingleton(new StorageClientConfig
        {
            EventQueueStorageAccountName = storageQueue.Credentials.AccountName,
            EventQueueStorageAccountKey = Convert.ToBase64String(storageQueue.Credentials.ExportKey())
        });

        services.AddDefaultRules();

        services.AddSingleton<IPipelineRegistrationRepository>(_ => new PipelineRegistrationRepository(() => cloudTableClient));
        services.AddSingleton<IPipelineRegistrationResolver, PipelineRegistrationResolver>();
        services.AddTransient<IPipelineRegistrationMapper, PipelineRegistrationMapper>();
        services.AddSingleton<IPipelineRegistrationStorageRepository>(s =>
            new PipelineRegistrationStorageRepository(
                s.GetRequiredService<ILogger<PipelineRegistrationStorageRepository>>(),
                () => cloudTableClient));

        services.AddScoped<IClassicReleaseDeploymentEventParser, ClassicReleaseDeploymentEventParser>();
        services.AddScoped<IMonitorDecoratorService, MonitorDecoratorService>();
        services.AddSingleton<IYamlReleaseDeploymentEventParser, YamlReleaseDeploymentEventParser>();
        services.AddSingleton<IPullRequestMergedEventParser, PullRequestMergedEventParser>();
        services.AddSingleton<IReleasePipelineService, ReleasePipelineService>();
        services.AddSingleton<IBuildPipelineService, BuildPipelineService>();
        services.AddSingleton<IRepositoryService, RepositoryService>();
        services.AddSingleton<IManageHooksService, ManageHooksService>();
        services.AddSingleton<IPipelineEvaluatorFactory, PipelineEvaluatorFactory>();
        services.AddSingleton<IYamlHelper, YamlHelper>();
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");

    private static Task CreateQueuesIfNotExists(CloudQueueClient cloudQueueClient)
    {
        var tasks = new List<Task>
        {
            cloudQueueClient.GetQueueReference(StorageQueueNames.AuditClassicReleaseQueueName).CreateIfNotExistsAsync(),
            cloudQueueClient.GetQueueReference(StorageQueueNames.AuditYamlReleaseQueueName).CreateIfNotExistsAsync(),
            cloudQueueClient.GetQueueReference(StorageQueueNames.AuditPullRequestApproversQueueName).CreateIfNotExistsAsync(),
            cloudQueueClient.GetQueueReference(StorageQueueNames.DeviationReportQueueName).CreateIfNotExistsAsync()
        };
        return Task.WhenAll(tasks);
    }
}