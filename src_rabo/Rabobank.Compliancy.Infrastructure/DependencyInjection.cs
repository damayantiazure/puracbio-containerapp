#nullable enable

using AutoMapper;
using Azure.Identity;
using Azure.Monitor.Query;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Application.Security;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDataTablesClient;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;
using Rabobank.Compliancy.Clients.AzureDevopsClient;
using Rabobank.Compliancy.Clients.AzureQueueClient;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.InternalServices;
using Rabobank.Compliancy.Infrastructure.Mapping;
using Rabobank.Compliancy.Infrastructure.MonitoringDashboard;
using Rabobank.Compliancy.Infrastructure.Permissions;
using Rabobank.Compliancy.Infrastructure.Permissions.Context;
using Rabobank.Compliancy.Infrastructure.Security;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddCommonInfraDependencies(this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddAutoMapper(typeof(DeviationReportLogRecordMappingProfile))
            .AddLoggingInfraDependencies(configuration);

    public static IServiceCollection AddClientInfraDependencies(this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddCommonInfraDependencies(configuration)
            .AddAzdoClientDependencies()
            .AddAzureDataTablesClientDependencies()
            .AddQueueClientDependencies();

    public static IServiceCollection AddScanningInfraDependencies(this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddClientInfraDependencies(configuration)
            .AddSingleton(new AzureDevOpsExtensionConfig
            {
                ExtensionName = GetEnvironmentVariable("extensionName")
            })
            .AddScoped<IDeviationService>(s => new DeviationService(
                s.GetRequiredService<ITableStore<DeviationEntity>>,
                s.GetRequiredService<IQueueClientFacade>(),
                s.GetRequiredService<IMapper>()))
            .AddScoped<ICompliancyReportService, CompliancyReportService>();

    public static IServiceCollection AddMonitoringDashboardTileServiceDependencies(this IServiceCollection services) =>
        services.AddScoped<IMonitoringDashboardTileService, MonitoringDashboardTileService>();

    public static IServiceCollection AddApplicationInfraDependencies(this IServiceCollection services,
        IConfiguration configuration) =>
        services
            .AddConfigurations()
            .AddScanningInfraDependencies(configuration)
            .AddScoped<IGateService, GateService>()
            .AddScoped<IProjectService, ProjectService>()
            .AddScoped<IGitRepoService, GitRepoService>()
            .AddScoped<IPipelineService, PipelineService>()
            .AddScoped<IAuthorizationService, AuthorizationService>()
            .AddScoped<IPipelineRegistrationService>(s =>
                new PipelineRegistrationService(s.GetRequiredService<ITableStore<DeploymentMethodEntity>>))
            .AddScoped<IDeviationLogService, DeviationLogService>()
            .AddScoped<IExclusionService>(s => new ExclusionService(
                s.GetRequiredService<ITableStore<ExclusionEntity>>,
                s.GetRequiredService<IMapper>()))
            .AddScoped<IProtectedResourcePermissionsService, ProtectedResourcePermissionsService>()
            .AddScoped<IPermissionContextFactory, PermissionContextFactory>()
            .AddScoped<IPermissionGroupService, PermissionGroupService>()
            .AddScoped<IPermissionsHandler<GitRepo>, RepositoryPermissionsHandler>()
            .AddScoped<IPermissionsHandler<Pipeline>, PipelinePermissionsHandler>()
            .AddScoped<ISecurityContext, SecurityContext>();

    private static IServiceCollection AddLoggingInfraDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        var logIngestionSection = configuration.GetRequiredSection("logging");
        var logConfig = logIngestionSection.Get<LogConfig>();

        return services
            .AddSingleton(logConfig)
            .AddScoped(_ => new LogsQueryClient(new DefaultAzureCredential()))
            .AddSingleton<ILogIngestionService, LogIngestionService>()
            .AddScoped<ILogQueryService>(s => new LogQueryService(
                s.GetRequiredService<LogsQueryClient>(),
                s.GetRequiredService<LogConfig>()))
            .AddSingleton<IIngestionClientFactory>(s =>
                new IngestionClientFactory(new DefaultAzureCredential(), s.GetRequiredService<LogConfig>().Ingestion))
            .AddSingleton<ILoggingService, LoggingService>();
    }

    private static IServiceCollection AddConfigurations(this IServiceCollection services) =>
        services
            .AddSingleton(new AzureDevOpsExtensionConfig
            {
                ExtensionName = GetEnvironmentVariable("extensionName")
            });

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            $"Please provide a valid value for environment variable '{variableName}'");
}