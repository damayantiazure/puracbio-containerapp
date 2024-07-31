using Azure.Monitor.Ingestion;
using Rabobank.Compliancy.Infrastructure.Config;

namespace Rabobank.Compliancy.Infrastructure.InternalServices;

public class IngestionClientFactoryResult
{
    public IngestionClientFactoryResult(LogsIngestionClient client, LogIngestionClientConfig clientConfig)
    {
        Client = client;
        ClientConfig = clientConfig;
    }

    public LogsIngestionClient Client { get; }
    public LogIngestionClientConfig ClientConfig { get; }
}