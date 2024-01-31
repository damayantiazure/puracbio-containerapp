using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.StorageClient;

public interface IPipelineRegistrationResolver
{
    Task<IEnumerable<string>> ResolveProductionStagesAsync(string organization, string projectId, string pipelineId);
}