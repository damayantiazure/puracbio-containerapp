using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public interface IYamlHelper
{
    Task<IEnumerable<PipelineTaskInputs>> GetPipelineTasksAsync(string organization, string projectId, BuildDefinition buildPipeline);
}