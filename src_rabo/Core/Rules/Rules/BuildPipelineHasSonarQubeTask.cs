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

public class BuildPipelineHasSonarqubeTask : BuildPipelineRule, IBuildPipelineRule
{
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildPipelineHasSonarqubeTask(IAzdoRestClient client, IMemoryCache cache, IYamlHelper yamlHelper) : base(client) =>
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(client, cache, yamlHelper);


    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("6d01813a-9589-4b15-8491-8164aeb38055")
        {
            TaskName = "SonarQubeAnalyze",
        },
        new PipelineHasTaskRule("ac4ee482-65da-4485-a532-7b085873e532")
        {
            TaskName = "GuiMavenTask",
            Inputs = new Dictionary<string, string>{{ "sqAnalysisEnabled", "true" }}
        },
        new PipelineHasTaskRule("YamlMavenTask")
        {
            TaskName = "Maven",
            Inputs = new Dictionary<string, string>{{ "sonarQubeRunAnalysis", "true" }}
        }
    };
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.BuildPipelineHasSonarqubeTask;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Build pipeline contains SonarQube task";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/RShFD";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.SecurityTesting };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, Response.BuildDefinition buildPipeline)
    {
        var result = await Task.WhenAll(_rules.Select(async r =>
            await _pipelineEvaluatorFactory
                .EvaluateBuildTaskAsync(r, organization, projectId, buildPipeline)));
        return result.Any(x => x);
    }
}