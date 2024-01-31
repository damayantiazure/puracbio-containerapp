using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class ClassicReleasePipelineHasSm9ChangeTask : ClassicReleasePipelineRule, IClassicReleasePipelineRule
{
    private readonly ClassicPipelineEvaluator _pipelineEvaluator;

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

    public ClassicReleasePipelineHasSm9ChangeTask(IAzdoRestClient client, IMemoryCache cache) : base(client)
    {
        _pipelineEvaluator = new ClassicPipelineEvaluator(client, cache);
    }

    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.ClassicReleasePipelineHasSm9ChangeTask;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Classic release pipeline contains SM9 Change task";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/NRV1D";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles =>
        new[] { BluePrintPrinciples.Auditability };

    public override Task<bool> EvaluateAsync(
        string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }
        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }
        if (releasePipeline == null)
        {
            throw new ArgumentNullException(nameof(releasePipeline));
        }
        if (releasePipeline.Environments == null)
        {
            throw new ArgumentOutOfRangeException(nameof(releasePipeline));
        }

        return EvaluateInternalAsync(organization, projectId, releasePipeline);
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, ReleaseDefinition releasePipeline)
    {
        var tasks = releasePipeline.Environments
            .SelectMany(e => e.DeployPhases)
            .SelectMany(d => d.WorkflowTasks)
            .ToList();

        var found = _rules.Select(r => PipelineContainsTask(tasks, r)).Any(x => x);

        var queue = GetTaskGroups(tasks).ToList();

        var result = await System.Threading.Tasks.Task.WhenAll(_rules.Select(async r =>
            await _pipelineEvaluator.EvaluateTaskGroupsAsync(
                found, queue, organization, projectId, r)));

        return result.Any(x => x);
    }

    private static bool PipelineContainsTask(IEnumerable<WorkflowTask> tasks, IPipelineHasTaskRule rule) =>
        tasks.Any(t => t.Enabled && t.TaskId.ToString() == rule.TaskId);

    private static IEnumerable<string> GetTaskGroups(IEnumerable<WorkflowTask> tasks) =>
        tasks.Where(t => t.Enabled && t.DefinitionType == "metaTask")
            .Select(t => t.TaskId.ToString());
}