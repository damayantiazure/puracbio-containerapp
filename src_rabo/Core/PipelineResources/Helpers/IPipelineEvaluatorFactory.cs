using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public interface IPipelineEvaluatorFactory
{
    IPipelineEvaluator Create(BuildDefinition buildPipeline);

    Task<bool> EvaluateBuildTaskAsync(IPipelineHasTaskRule rule
        , string organization, string projectId
        , BuildDefinition buildPipeline);

    Task<IEnumerable<BuildDefinition>> GetPipelinesAsync(string organization
        , IEnumerable<BuildDefinition> allBuildPipelines, string projectId
        , BuildDefinition pipeline, IPipelineHasTaskRule[] rules);
}