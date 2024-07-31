using Microsoft.Extensions.DependencyInjection;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.DeploymentMethods;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Deviations;
using Rabobank.Compliancy.Clients.AzureDataTablesClient.Exclusions;
using System.Globalization;
using TableStorage.Abstractions.Store;

namespace Rabobank.Compliancy.Clients.AzureDataTablesClient;

/// <summary>
/// This Extension Class is responsible for the dependency injection of everything a project using this project needs
/// </summary>
public static class DependencyInjection
{
    private const string _environmentVariableNotFoundError = "Please provide a valid value for environment variable {0}";

    public static IServiceCollection AddAzureDataTablesClientDependencies(this IServiceCollection services)
    {
        var connectionString = GetEnvironmentVariable("tableStorageConnectionString");

        services.AddSingleton<ITableStore<DeviationEntity>>(_ => new TableStore<DeviationEntity>("Deviations", connectionString));
        services.AddSingleton<ITableStore<DeploymentMethodEntity>>(_ => new TableStore<DeploymentMethodEntity>("DeploymentMethods", connectionString));
        services.AddSingleton<ITableStore<ExclusionEntity>>(_ => new TableStore<ExclusionEntity>("ExclusionList", connectionString));

        return services;
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName, string.Format(CultureInfo.InvariantCulture, _environmentVariableNotFoundError, variableName));
}