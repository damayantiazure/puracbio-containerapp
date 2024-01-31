using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public interface IReleasePipelineService
{
    public Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, ReleaseDefinition releasePipeline, string projectId,
        IEnumerable<BuildDefinition> allBuildPipelines = null);

    public Task<IEnumerable<Repository>> GetLinkedRepositoriesAsync(string organization,
        IEnumerable<ReleaseDefinition> releasePipelines, IEnumerable<BuildDefinition> buildPipelines);
}