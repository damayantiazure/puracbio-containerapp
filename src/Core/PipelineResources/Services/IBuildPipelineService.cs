using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public interface IBuildPipelineService
{
    Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, BuildDefinition yamlReleasePipeline, IEnumerable<BuildDefinition> allBuildPipelines = null);

    Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, IEnumerable<BuildDefinition> buildPipelines, IEnumerable<BuildDefinition> allBuildPipelines = null);

    Task<IEnumerable<Repository>> GetLinkedRepositoriesAsync(string organization, IEnumerable<BuildDefinition> buildPipelines);
}