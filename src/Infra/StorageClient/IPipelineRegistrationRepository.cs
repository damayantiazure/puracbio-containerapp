using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.StorageClient;

public interface IPipelineRegistrationRepository
{
    Task<List<PipelineRegistration>> GetAsync(string organization, string projectId);

    Task<List<PipelineRegistration>> GetAsync(string organization, string projectId, string ciIdentifier);

    Task<List<PipelineRegistration>> GetAsync(string organization, string projectId, string pipelineId, string stageId);

    Task<List<PipelineRegistration>> GetAsync(GetPipelineRegistrationRequest request);
}