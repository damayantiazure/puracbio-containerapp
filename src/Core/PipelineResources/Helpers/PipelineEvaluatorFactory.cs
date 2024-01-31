using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public class PipelineEvaluatorFactory : IPipelineEvaluatorFactory
{
    private readonly Dictionary<int, IPipelineEvaluator> _evaluators;

    public PipelineEvaluatorFactory(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper)
    {
        _evaluators = new Dictionary<int, IPipelineEvaluator>
        {
            [PipelineProcessType.GuiPipeline] = new ClassicPipelineEvaluator(client, cache),
            [PipelineProcessType.YamlPipeline] = new YamlPipelineEvaluator(client, yamlHelper)
        };
    }

    public IPipelineEvaluator Create(BuildDefinition buildPipeline)
    {
        if (buildPipeline == null)
        {
            throw new ArgumentNullException(nameof(buildPipeline));
        }

        if (!_evaluators.ContainsKey(buildPipeline.Process.Type))
        {
            throw new ArgumentOutOfRangeException(nameof(buildPipeline));
        }

        return _evaluators[buildPipeline.Process.Type];
    }

    public async Task<bool> EvaluateBuildTaskAsync(IPipelineHasTaskRule rule,
        string organization, string projectId, BuildDefinition buildPipeline) =>
        await Create(buildPipeline)
            .EvaluateAsync(organization, projectId, buildPipeline, rule);

    public async Task<IEnumerable<BuildDefinition>> GetPipelinesAsync(string organization,
        IEnumerable<BuildDefinition> allBuildPipelines, string projectId, BuildDefinition pipeline, IPipelineHasTaskRule[] rules) =>
        await Create(pipeline).GetPipelinesAsync(organization, allBuildPipelines, projectId, pipeline, rules);
}