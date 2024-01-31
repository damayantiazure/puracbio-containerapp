using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class BuildArtifactIsStoredSecure : BuildPipelineRule, IBuildPipelineRule
{
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildArtifactIsStoredSecure(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper) : base(client) =>
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client, cache, yamlHelper);


    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("2ff763a7-ce83-4e1f-bc89-0ae63477cebe")
        {
            TaskName = "PublishBuildArtifacts"
        },
        new PipelineHasTaskRule("ecdc45f6-832d-4ad9-b52b-ee49e94659be")
        {
            TaskName = "PublishPipelineArtifact"
        }
    };

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.BuildArtifactIsStoredSecure;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Artifact is stored in Azure DevOps";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/TI8AD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.CodeIntegrity };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, BuildDefinition buildPipeline)
    {
        foreach (var rule in _rules)
        {
            var result = await _pipelineEvaluatorFactory.Create(buildPipeline)
                .EvaluateAsync(organization, projectId, buildPipeline, rule);

            if (result)
            {
                return true;
            }
        }
        return false;
    }
}