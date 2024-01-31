#nullable enable

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Infra.StorageClient;

public class PipelineRegistrationResolver : IPipelineRegistrationResolver
{
    private readonly IPipelineRegistrationRepository _pipelineRegistrationRepository;

    public PipelineRegistrationResolver(IPipelineRegistrationRepository pipelineRegistrationRepository) => 
        _pipelineRegistrationRepository = pipelineRegistrationRepository;

    public async Task<IEnumerable<string>> ResolveProductionStagesAsync(string organization, string projectId, string pipelineId)
    {
        var pipelineRegistrations = await _pipelineRegistrationRepository.GetAsync(organization, projectId);

        return pipelineRegistrations
            .Where(pipelineRegistration => 
                pipelineRegistration.PipelineId == pipelineId && 
                !string.IsNullOrEmpty(pipelineRegistration.StageId))
            .Select(pipelineRegistration => pipelineRegistration.StageId)
            .Distinct()
            .ToList();
    }
}