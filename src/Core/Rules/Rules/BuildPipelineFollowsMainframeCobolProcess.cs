using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class BuildPipelineFollowsMainframeCobolProcess : BuildPipelineRule, IBuildPipelineRule
{
    private readonly IPipelineEvaluatorFactory _pipelineEvaluatorFactory;

    private readonly IPipelineHasTaskRule[] _classicRules =
    {
        new PipelineHasTaskRule(TaskContants.MainframeCobolConstants.DbbBuildTaskId)
        {
            TaskName = TaskContants.MainframeCobolConstants.DbbBuildTaskName,
        },
        new PipelineHasTaskRule(TaskContants.MainframeCobolConstants.DbbPackageTaskId)
        {
            TaskName = TaskContants.MainframeCobolConstants.DbbPackageTaskName,
        },
    };

    private readonly IPipelineHasTaskRule[] _ymlRules =
    {
        new PipelineHasTaskRule(TaskContants.MainframeCobolConstants.DbbBuildTaskId)
        {
            TaskName = TaskContants.MainframeCobolConstants.DbbBuildTaskName,
        },
        new PipelineHasTaskRule(TaskContants.MainframeCobolConstants.DbbPackageTaskId)
        {
            TaskName = TaskContants.MainframeCobolConstants.DbbPackageTaskName,
        }
    };

    public BuildPipelineFollowsMainframeCobolProcess(IAzdoRestClient client, IPipelineEvaluatorFactory pipelineEvaluatorFactory) : base(client) =>
        _pipelineEvaluatorFactory = pipelineEvaluatorFactory;

    [ExcludeFromCodeCoverage]
    public string Name => RuleNames.BuildPipelineFollowsMainframeCobolProcess;

    [ExcludeFromCodeCoverage]
    public string Description => "Build pipeline follows mainframe Cobol build process";

    [ExcludeFromCodeCoverage]
    public string Link => "https://confluence.dev.rabobank.nl/x/NRV1D";

    [ExcludeFromCodeCoverage]
    public BluePrintPrinciple[] Principles => new[] { BluePrintPrinciples.CodeIntegrity, BluePrintPrinciples.SecurityTesting };

    public override async Task<bool> EvaluateAsync(string organization, string projectId, BuildDefinition buildPipeline)
    {
        ValidateInput(organization, projectId, buildPipeline);

        return await EvaluateInternalAsync(organization, projectId, buildPipeline);
    }

    private static void ValidateInput(string organization, string projectId, BuildDefinition buildPipeline)
    {
        if (organization.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(organization));
        }
        if (projectId.IsNullOrWhiteSpace())
        {
            throw new ArgumentNullException(nameof(projectId));
        }
        if (buildPipeline == null)
        {
            throw new ArgumentNullException(nameof(buildPipeline));
        }
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, BuildDefinition buildPipeline)
    {
        if (buildPipeline.UsedYaml.IsNotNullOrWhiteSpace())
        {
            // Yaml
            var result = await Task.WhenAll(_ymlRules.Select(async r =>
                await _pipelineEvaluatorFactory
                    .EvaluateBuildTaskAsync(r, organization, projectId, buildPipeline)));
            return result.All(x => x);
        }
        else
        {
            // Classic
            var result = await Task.WhenAll(_classicRules.Select(async r =>
                await _pipelineEvaluatorFactory
                    .EvaluateBuildTaskAsync(r, organization, projectId, buildPipeline)));
            return result.All(x => x);
        }
    }
}