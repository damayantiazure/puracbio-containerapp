using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Requests = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public class ClassicPipelineEvaluator : IPipelineEvaluator
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IMemoryCache _cache;

    public ClassicPipelineEvaluator(IAzdoRestClient client, IMemoryCache cache)
    {
        _azdoClient = client;
        _cache = cache;
    }

    public Task<bool> EvaluateAsync(string organization, string projectId,
        BuildDefinition buildPipeline, IPipelineHasTaskRule rule)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (buildPipeline == null)
        {
            throw new ArgumentNullException(nameof(buildPipeline));
        }

        if (buildPipeline.Process.Phases == null)
        {
            throw new ArgumentOutOfRangeException(nameof(buildPipeline));
        }

        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }

        return BuildPipelineContainsTaskAsync(organization, projectId, buildPipeline, rule);
    }

    private Task<bool> BuildPipelineContainsTaskAsync(string organization,
        string projectId, BuildDefinition buildPipeline, IPipelineHasTaskRule rule)
    {
        var steps = buildPipeline.Process.Phases
            .Where(p => p.Steps != null)
            .SelectMany(p => p.Steps);

        var found = BuildStepsContainTaskAndValidInput(steps, rule);
        var queue = GetTaskGroupIds(steps);

        return EvaluateTaskGroupsAsync(found, queue, organization, projectId, rule);
    }

    private async Task<IEnumerable<BuildStep>> GetTaskGroupStepsAsync(string organization, string projectId, string taskGroup)
    {
        var endpoint = Requests.TaskGroup.TaskGroupById(projectId, taskGroup);

        // Cache the calls for retrieving a taskgroup by Id. The url of the endpoint will be the key of the cache.
        // The slidingExpiration value is choosen without a particular reason. 3 min seems well enough for now.
        var response = await _cache.GetOrCreate(endpoint.Url().ToString(),
            async entry =>
            {
                entry.SlidingExpiration = TimeSpan.FromSeconds(180);
                return (await _azdoClient.GetAsync(Requests.TaskGroup.TaskGroupById(projectId, taskGroup), organization))
                    .Value?.FirstOrDefault()?.Tasks ?? Enumerable.Empty<BuildStep>();
            });

        return response;
    }

    private static IEnumerable<string> GetTaskGroupIds(IEnumerable<BuildStep> steps) =>
        steps
            .Where(s => s.Enabled && s.Task.DefinitionType == "metaTask")
            .Select(s => s.Task.Id);

    private static bool BuildStepsContainTaskAndValidInput(IEnumerable<BuildStep> buildSteps, IPipelineHasTaskRule rule)
    {
        var listOfEnabledTasks = buildSteps.Where(s => s.Enabled && s.Task.Id == rule.TaskId);

        if (!listOfEnabledTasks.Any())
        {
            return false;
        }

        // verify if the rule has inputs that needs validations
        if (rule.Inputs == null || !rule.Inputs.Any())
        {
            return true;
        }

        // verify if the task contains specific inputs
        foreach (var enabledTask in listOfEnabledTasks)
        {
            var taskHasValidInputProperties = VerifyRuleInputs(enabledTask, rule);
            if (taskHasValidInputProperties)
            {
                return true;
            }
        }

        return false;
    }

    private static bool VerifyRuleInputs(BuildStep buildStep, IPipelineHasTaskRule pipelineHasTaskRule)
    {
        if (buildStep.Inputs == null)
        {
            return false;
        }

        foreach (var ruleInput in pipelineHasTaskRule.Inputs)
        {
            if (!buildStep.Inputs.ContainsKey(ruleInput.Key))
            {
                return false;
            }

            var stepInputValue = buildStep.Inputs[ruleInput.Key];
            // also check input values if ignoreinputvalues is false
            if (!pipelineHasTaskRule.IgnoreInputValues && !string.IsNullOrEmpty(stepInputValue))
            {
                if (stepInputValue.Equals(ruleInput.Value, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return false;
            }
        }

        return true;
    }

    public async Task<bool> EvaluateTaskGroupsAsync(bool found, IEnumerable<string> queue,
        string organization, string projectId, IPipelineHasTaskRule rule)
    {
        var done = new HashSet<string>();
        while (!found && queue.Any())
        {
            var todo = queue.Where(q => !done.Contains(q));
            var buildSteps = (await Task.WhenAll(todo.Select(q =>
                    GetTaskGroupStepsAsync(organization, projectId, q))))
                .SelectMany(s => s);
            found = BuildStepsContainTaskAndValidInput(buildSteps, rule);
            done.UnionWith(queue);
            queue = GetTaskGroupIds(buildSteps);
        }
        return found;
    }

    public async Task<IEnumerable<BuildDefinition>> GetPipelinesAsync(string organization,
        IEnumerable<BuildDefinition> allBuildPipelines, string projectId, BuildDefinition pipeline,
        IPipelineHasTaskRule[] rules)
    {
        if (pipeline.Process == null)
        {
            return new List<BuildDefinition>();
        }

        var steps = pipeline.Process.Phases
            .Where(p => p.Steps != null)
            .SelectMany(p => p.Steps);

        var queue = GetTaskGroupIds(steps);

        var linkedPipelinesFromTasks = await GetPipelinesFromTaskAsync(steps, organization, allBuildPipelines, rules);
        var linkedPipelinesFromTaskGroups = await GetPipelinesFromTaskGroupsAsync(
            queue, organization, projectId, allBuildPipelines, rules);

        return linkedPipelinesFromTasks.Concat(linkedPipelinesFromTaskGroups);
    }

    private async Task<IEnumerable<BuildDefinition>> GetPipelinesFromTaskAsync(IEnumerable<BuildStep> steps,
        string organization, IEnumerable<BuildDefinition> allBuildPipelines, IPipelineHasTaskRule[] rules)
    {
        var pipelinesFromPipelineArtifactTask = await Task.WhenAll(steps
            .Where(s => s != null &&
                        s.Enabled &&
                        s.Task.Id == rules.First().TaskId &&
                        VerifyRuleInputs(s, rules.First()))
            .Select(async s => await GetPipelineAsync(organization, allBuildPipelines,
                s.Inputs["project"], s.Inputs["pipeline"])));

        var pipelinesFromBuildArtifactTask = await Task.WhenAll(steps
            .Where(s => s != null &&
                        s.Enabled &&
                        s.Task.Id == rules[1].TaskId &&
                        VerifyRuleInputs(s, rules[1]))
            .Select(async s => await GetPipelineAsync(organization, allBuildPipelines,
                s.Inputs["project"], s.Inputs["definition"])));

        return pipelinesFromPipelineArtifactTask
            .Concat(pipelinesFromBuildArtifactTask);
    }

    public async Task<BuildDefinition> GetPipelineAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        string projectId, string itemId)
    {
        var pipeline = allBuildPipelines?.FirstOrDefault(d => d.Project.Id == projectId && d.Id == itemId);

        return pipeline ?? await _azdoClient.GetAsync(Requests.Builds.BuildDefinition(projectId, itemId), organization);
    }

    public async Task<IEnumerable<BuildDefinition>> GetPipelinesFromTaskGroupsAsync(
        IEnumerable<string> queue, string organization, string projectId, IEnumerable<BuildDefinition> allBuildPipelines, IPipelineHasTaskRule[] rules)
    {
        var done = new HashSet<string>();
        var buildPipelines = new List<BuildDefinition>();

        while (queue.Any())
        {
            var todo = queue.Where(q => !done.Contains(q));
            var buildSteps = (await Task.WhenAll(todo
                    .Select(q => GetTaskGroupStepsAsync(organization, projectId, q))))
                .SelectMany(s => s);
            buildPipelines.AddRange(await GetPipelinesFromTaskAsync(buildSteps, organization, allBuildPipelines, rules));
            done.UnionWith(queue);
            queue = GetTaskGroupIds(buildSteps);
        }
        return buildPipelines;
    }
}