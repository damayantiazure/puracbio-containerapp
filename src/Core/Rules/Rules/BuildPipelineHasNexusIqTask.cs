using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class BuildPipelineHasNexusIqTask : BuildPipelineRule, IPipelineHasTaskRule, IBuildPipelineRule
{
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildPipelineHasNexusIqTask(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper) : base(client) =>
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client, cache, yamlHelper);

    public string TaskId => "4f40d1a2-83b0-4ddc-9a77-e7f279eb1802";
    public string TaskName => "NexusIqPipelineTask";
    public Dictionary<string, string> Inputs => new Dictionary<string, string>();
    public bool IgnoreInputValues { get; init; }

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.BuildPipelineHasNexusIqTask;

    [ExcludeFromCodeCoverage]
    string IRule.Description => "Build pipeline contains NexusIQ task";

    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/JSNFD";

    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.SecurityTesting };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, BuildDefinition buildPipeline) =>
        await _pipelineEvaluatorFactory
            .EvaluateBuildTaskAsync(this, organization, projectId, buildPipeline);
}