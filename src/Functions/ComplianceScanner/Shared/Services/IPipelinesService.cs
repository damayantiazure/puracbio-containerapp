using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public interface IPipelinesService
{
    Task<IEnumerable<ReleaseDefinition>> GetClassicReleasePipelinesAsync(string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations);

    Task<IEnumerable<BuildDefinition>> GetAllYamlPipelinesAsync(string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations);

    Task<IEnumerable<BuildDefinition>> GetClassicBuildPipelinesAsync(string organization, string projectId);
}