using Azure.Core;
using Azure.Monitor.Ingestion;
using Rabobank.Compliancy.Infrastructure.Config;
using Rabobank.Compliancy.Infrastructure.InternalContracts;

namespace Rabobank.Compliancy.Infrastructure.InternalServices;

public class IngestionClientFactory : IIngestionClientFactory
{
    private readonly Dictionary<string, IngestionClientFactoryResult> _cache = new();
    private readonly LogIngestionConfig _logIngestionConfig;
    private readonly TokenCredential _tokenCredential;

    public IngestionClientFactory(TokenCredential tokenCredential, LogIngestionConfig logIngestionConfig)
    {
        _tokenCredential = tokenCredential;
        _logIngestionConfig = logIngestionConfig;
    }

    public IngestionClientFactoryResult Create(string modelName)
    {
        if (_cache.TryGetValue(modelName, out var cachedClient))
        {
            return cachedClient;
        }

        var client = new LogsIngestionClient(_logIngestionConfig.EndPoint, _tokenCredential);
        var clientConfig =
            _logIngestionConfig.Clients.Single(ingestionClientConfig => ingestionClientConfig.ModelName == modelName);

        var result = new IngestionClientFactoryResult(client, clientConfig);

        _cache[modelName] = result;

        return result;
    }
}