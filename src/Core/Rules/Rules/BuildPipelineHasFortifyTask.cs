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

public class BuildPipelineHasFortifyTask : BuildPipelineRule, IPipelineHasTaskRule, IBuildPipelineRule
{
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildPipelineHasFortifyTask(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper) : base(client) =>
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client, cache, yamlHelper);

    public string TaskId => "818386e5-c8a5-46c3-822d-954b3c8fb130";
    public string TaskName => "FortifySCA";
    public Dictionary<string, string> Inputs => new Dictionary<string, string>();
    public bool IgnoreInputValues { get; init; }

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.BuildPipelineHasFortifyTask;

    [ExcludeFromCodeCoverage]
    string IRule.Description => "Build pipeline contains Fortify task";

    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/9w1TD";

    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.SecurityTesting };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, BuildDefinition buildPipeline) =>
        await _pipelineEvaluatorFactory
            .EvaluateBuildTaskAsync(this, organization, projectId, buildPipeline);
}