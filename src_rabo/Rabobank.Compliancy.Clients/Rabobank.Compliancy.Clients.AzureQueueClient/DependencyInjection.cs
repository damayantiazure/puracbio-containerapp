using Azure.Storage.Queues;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Rabobank.Compliancy.Clients.AzureQueueClient;

public static class DependencyInjection
{
    private const string InvalidConfigurationError = "Please provide a valid value for environment variable '{0}'";
    // This queuename is also present in the (yet not refactored) StorageClient class from infra
    private const string DeviationLogRecordsStorageQueueName = "deviationreportlogrecords";

    public static IServiceCollection AddQueueClientDependencies(this IServiceCollection services)
    {
        services.AddAzureClients(builder =>
        {
            builder.AddClient((QueueClientOptions clientOptions, Azure.Core.TokenCredential _, IServiceProvider _) =>
            {
                clientOptions.MessageEncoding = QueueMessageEncoding.Base64;
                var connectionString = GetEnvironmentVariable("auditLoggingEventQueueStorageConnectionString");
                return new QueueClient(connectionString, DeviationLogRecordsStorageQueueName, clientOptions);
            });
        });

        services.AddSingleton<IQueueClientFacade, QueueClientFacade>();
        return services;
    }

    private static string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new ArgumentNullException(variableName,
            string.Format(CultureInfo.InvariantCulture, InvalidConfigurationError, variableName));
}