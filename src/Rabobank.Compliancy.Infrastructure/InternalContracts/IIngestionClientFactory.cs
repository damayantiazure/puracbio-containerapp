using Rabobank.Compliancy.Infrastructure.InternalServices;

namespace Rabobank.Compliancy.Infrastructure.InternalContracts;

public interface IIngestionClientFactory
{
    IngestionClientFactoryResult Create(string modelName);
}