using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Response = Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class BuildPipelineHasCredScanTask : BuildPipelineRule, IBuildPipelineRule
{
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildPipelineHasCredScanTask(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper) : base(client) =>
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client, cache, yamlHelper);


    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("f0462eae-4df1-45e9-a754-8184da95ed01")
        {
            TaskName = "CredScan",
        },
        new PipelineHasTaskRule("dbe519ee-a2e4-43f5-8e1a-949bd935b736")
        {
            TaskName = "PostAnalysis",
            Inputs = new Dictionary<string, string>{{"CredScan", "true"}}
        }
    };
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.BuildPipelineHasCredScanTask;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Build pipeline contains CredScan task";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/LorHDQ";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.SecurityTesting };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, Response.BuildDefinition buildPipeline)
    {
        var result = await Task.WhenAll(_rules.Select(async r =>
            await _pipelineEvaluatorFactory
                .EvaluateBuildTaskAsync(r, organization, projectId, buildPipeline)));

        return result.All(x => x);
    }
}