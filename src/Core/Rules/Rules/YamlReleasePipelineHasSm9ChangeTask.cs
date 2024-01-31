using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Tasks = System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class YamlReleasePipelineHasSm9ChangeTask : BuildPipelineRule, IYamlReleasePipelineRule
{
    public YamlReleasePipelineHasSm9ChangeTask(IAzdoRestClient client, IYamlHelper yamlHelper) : base(client) 
    {
        _pipelineEvaluator = new YamlPipelineEvaluator(client, yamlHelper);
    }

    private readonly YamlPipelineEvaluator _pipelineEvaluator;

    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("d0c045b6-d01d-4d69-882a-c21b18a35472")
        {
            TaskName = "SM9 - Create",
        },
        new PipelineHasTaskRule("73cb0c6a-0623-4814-8774-57dc1ef33858")
        {
            TaskName = "SM9 - Approve",
        }
    };

    public static Dictionary<string, string> Inputs => new Dictionary<string, string>();

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.YamlReleasePipelineHasSm9ChangeTask;

    [ExcludeFromCodeCoverage]
    string IRule.Description => "YAML release pipeline contains SM9 Change task";

    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/vDSgDw";

    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    public async override Tasks.Task<bool> EvaluateAsync(
        string organization, string projectId, BuildDefinition buildPipeline)
    {
        var result = await Tasks.Task.WhenAll(_rules.Select(r =>
            _pipelineEvaluator.EvaluateAsync(organization, projectId, buildPipeline, r)));

        return result.Any(x => x);
    }
}