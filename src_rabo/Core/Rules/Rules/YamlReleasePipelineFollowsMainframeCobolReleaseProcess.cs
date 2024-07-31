using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class YamlReleasePipelineFollowsMainframeCobolReleaseProcess : BuildPipelineRule, IYamlReleasePipelineRule
{
    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule(TaskContants.MainframeCobolConstants.DbbDeployTaskId)
        {
            TaskName = TaskContants.MainframeCobolConstants.DbbDeployTaskName,
            Inputs = new Dictionary<string, string>
            {
                { TaskContants.MainframeCobolConstants.OrganizationName, null},
                { TaskContants.MainframeCobolConstants.ProjectId, null },
                { TaskContants.MainframeCobolConstants.PipelineId, null }
            },
            IgnoreInputValues = true
        }
    };

    private readonly IPipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public YamlReleasePipelineFollowsMainframeCobolReleaseProcess(IAzdoRestClient azdoClient, IPipelineEvaluatorFactory pipelineEvaluatorFactory) : base(azdoClient)
    {
        _pipelineEvaluatorFactory = pipelineEvaluatorFactory;
    }

    [ExcludeFromCodeCoverage]
    public string Name => RuleNames.YamlReleasePipelineFollowsMainframeCobolReleaseProcess;

    [ExcludeFromCodeCoverage]
    public string Description => "Yaml release pipeline follows mainframe Cobol release process";

    [ExcludeFromCodeCoverage]
    public string Link => "https://confluence.dev.rabobank.nl/x/sDSgDw";

    [ExcludeFromCodeCoverage]
    public BluePrintPrinciple[] Principles => new[] { BluePrintPrinciples.CodeIntegrity };

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
        var pipelineEvaluator = _pipelineEvaluatorFactory.Create(buildPipeline);
        return (await Task.WhenAll(_rules.Select(r =>
            pipelineEvaluator.EvaluateAsync(organization, projectId, buildPipeline, r)))).Any(x => x);
    }
}