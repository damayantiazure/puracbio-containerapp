using Rabobank.Compliancy.Core.PipelineResources.Model;
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

public class ClassicReleasePipelineFollowsMainframeCobolReleaseProcess : ClassicReleasePipelineRule, IClassicReleasePipelineRule
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

    public ClassicReleasePipelineFollowsMainframeCobolReleaseProcess(IAzdoRestClient azdoClient) : base(azdoClient)
    {
    }

    [ExcludeFromCodeCoverage]
    public string Name => RuleNames.ClassicReleasePipelineFollowsMainframeCobolReleaseProcess;

    [ExcludeFromCodeCoverage]
    public string Description => "Classic release pipeline follows mainframe Cobol process";

    [ExcludeFromCodeCoverage]
    public string Link => "https://confluence.dev.rabobank.nl/x/NRV1D";

    [ExcludeFromCodeCoverage]
    public BluePrintPrinciple[] Principles => new[] { BluePrintPrinciples.CodeIntegrity };

    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        ValidateInput(releasePipeline);

        return await EvaluateInternalAsync(releasePipeline);
    }

    private static void ValidateInput(ReleaseDefinition releaseDefinition)
    {
        if (releaseDefinition == null)
        {
            throw new ArgumentNullException(nameof(releaseDefinition));
        }
        if (releaseDefinition.Environments == null)
        {
            throw new ArgumentOutOfRangeException(nameof(releaseDefinition));
        }
    }

    private async Task<bool> EvaluateInternalAsync(ReleaseDefinition releaseDefinition)
    {
        var workFlowTasks = releaseDefinition.Environments
            .SelectMany(e => e.DeployPhases)
            .SelectMany(d => d.WorkflowTasks);

        return (await Task.WhenAll(_rules.Select(r => PipelineContainsTask(workFlowTasks, r))))
            .Any(x => x);
    }

    private static Task<bool> PipelineContainsTask(IEnumerable<WorkflowTask> workFlowTasks, IPipelineHasTaskRule rule)
    {
        var dbbDeployTasks = workFlowTasks.Where(t => t.Enabled
                                                      && t.TaskId.ToString() == rule.TaskId);

        if (dbbDeployTasks == null || !dbbDeployTasks.Any())
        {
            return Task.FromResult(false);
        }

        // Task is present and rule has no inputs to check for so return true
        if (rule.Inputs == null || !rule.Inputs.Any())
        {
            return Task.FromResult(true);
        }

        // Check inputs
        foreach (var task in dbbDeployTasks)
        {
            if (rule.Inputs.Keys.All(k => task.Inputs.TryGetValue(k, out var value) && value != String.Empty))
            {
                return Task.FromResult(true);
            }
        }

        return Task.FromResult(false);
    }
}