using Microsoft.Extensions.Caching.Memory;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public class ReleasePipelineService : PipelineServiceBase, IReleasePipelineService
{
    private const string ArtifactTypeBuild = "build";
    private const string ArtifactTypeRepo = "git";

    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("61f2a582-95ae-4948-b34d-a1b3c4f6a737")
        {
            TaskName = "DownloadPipelineArtifact",
            Inputs = new Dictionary<string, string>{{ "source", "specific"}}
        },
        new PipelineHasTaskRule("a433f589-fce1-4460-9ee6-44a624aeb1fb")
        {
            TaskName = "DownloadBuildArtifacts",
            Inputs = new Dictionary<string, string>{{ "buildType", "specific"}}
        }
    };

    private readonly ClassicPipelineEvaluator _pipelineEvaluator;
    private readonly IBuildPipelineService _buildPipelineService;

    public ReleasePipelineService(
        IAzdoRestClient azdoRestClient,
        IBuildPipelineService buildPipelineService,
        IMemoryCache cache) : base(azdoRestClient)
    {
        _pipelineEvaluator = new ClassicPipelineEvaluator(azdoRestClient, cache);
        _buildPipelineService = buildPipelineService;
    }

    public async Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, ReleaseDefinition releasePipeline, string projectId,
        IEnumerable<BuildDefinition> allBuildPipelines = null)
    {
        var artifactPipelines = await GetPipelinesFromArtifacts(organization, allBuildPipelines, releasePipeline);

        var downloadTaskPipelines = await GetPipelinesFromDownloadTasksAsync(organization, allBuildPipelines, releasePipeline, projectId);

        var mainframeCobolPipelines = await GetLinkedMainframeCobolPipelinesAsync(releasePipeline);

        var allLinkedPipelines = artifactPipelines.Concat(downloadTaskPipelines)
            .Concat(mainframeCobolPipelines);

        var profile = GetRuleProfileForYamlRelease(releasePipeline.PipelineRegistrations);
        releasePipeline.ProfileToApply = profile;

        foreach (var linkedPipelines in allLinkedPipelines)
        {
            linkedPipelines.ProfileToApply = profile;
        }

        return await _buildPipelineService.GetLinkedPipelinesAsync(
            organization, allLinkedPipelines.Distinct(), allBuildPipelines);
    }

    public async Task<IEnumerable<Repository>> GetLinkedRepositoriesAsync(string organization,
        IEnumerable<ReleaseDefinition> releasePipelines, IEnumerable<BuildDefinition> buildPipelines)
    {
        var reposLinkedToReleasePipeline = releasePipelines.SelectMany(r => GetArtifacts(r, ArtifactTypeRepo)
                .Select(a => new Repository
                {
                    Id = a.artifactId,
                    Name = a.artifactName,
                    Project = new Project
                    {
                        Id = a.projectId
                    },
                    Url = new Uri($"https://dev.azure.com/{organization}/{a.projectId}/_git/{a.artifactId}")
                }))
            .GroupBy(r => r.Url.AbsoluteUri)
            .Select(r => r.First());

        var reposLinkedToBuildPipelines = await _buildPipelineService.GetLinkedRepositoriesAsync(
            organization, buildPipelines);

        return reposLinkedToReleasePipeline
            .Concat(reposLinkedToBuildPipelines)
            .GroupBy(r => r.Id)
            .Select(r => r.First());
    }

    private async Task<IEnumerable<BuildDefinition>> GetPipelinesFromArtifacts(
        string organization, IEnumerable<BuildDefinition> allBuildPipelines, ReleaseDefinition releasePipeline)
    {
        var buildArtifacts = GetArtifacts(releasePipeline, ArtifactTypeBuild);

        return await Task.WhenAll(buildArtifacts
            .Select(async a => await _pipelineEvaluator.GetPipelineAsync(organization, allBuildPipelines, a.projectId, a.artifactId)));
    }

    private static IEnumerable<(string artifactId, string artifactName, string projectId)> GetArtifacts(
        ReleaseDefinition releasePipeline, string artifactType) =>
        releasePipeline.Artifacts
            .Where(artifact => artifact.Type.Equals(artifactType, StringComparison.InvariantCultureIgnoreCase))
            .Select(artifact => (artifact.DefinitionReference.Definition.Id, artifact.DefinitionReference.Definition.Name, artifact.DefinitionReference.Project.Id));

    private async Task<IEnumerable<BuildDefinition>> GetPipelinesFromDownloadTasksAsync(
        string organization, IEnumerable<BuildDefinition> allBuildPipelines, ReleaseDefinition releasePipeline, string projectId)
    {
        var tasks = releasePipeline.Environments
            .SelectMany(e => e.DeployPhases)
            .SelectMany(d => d.WorkflowTasks);

        var queue = GetTaskGroups(tasks);

        var buildPipelinesFromTasks = await GetPipelinesFromTaskAsync(tasks, organization, allBuildPipelines, _rules);
        var buildPipelinesFromTaskGroups = await _pipelineEvaluator.GetPipelinesFromTaskGroupsAsync(
            queue, organization, projectId, allBuildPipelines, _rules);

        return buildPipelinesFromTasks.Concat(buildPipelinesFromTaskGroups);
    }

    private static bool VerifyRuleInputs
        (WorkflowTask task, Dictionary<string, string> ruleInputs)
    {
        if (task.Inputs == null)
        {
            return false;
        }

        foreach (var ruleInput in ruleInputs)
        {
            if (!task.Inputs.ContainsKey(ruleInput.Key))
            {
                return false;
            }

            var taskInput = task.Inputs[ruleInput.Key];
            if (taskInput != null &&
                taskInput.Equals(ruleInput.Value, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

    private static IEnumerable<string> GetTaskGroups(IEnumerable<WorkflowTask> tasks) =>
        tasks.Where(t => t.Enabled && t.DefinitionType == "metaTask")
            .Select(t => t.TaskId.ToString());

    private async Task<IEnumerable<BuildDefinition>> GetPipelinesFromTaskAsync(
        IEnumerable<WorkflowTask> tasks, string organization, IEnumerable<BuildDefinition> allBuildPipelines, IPipelineHasTaskRule[] rules)
    {
        var pipelinesFromPipelineArtifactTask = await Task.WhenAll(tasks
            .Where(t => t != null &&
                        t.Enabled &&
                        t.TaskId.ToString() == rules.First().TaskId &&
                        VerifyRuleInputs(t, rules.First().Inputs))
            .Select(async t => await _pipelineEvaluator.GetPipelineAsync(organization, allBuildPipelines,
                t.Inputs["project"], t.Inputs["pipeline"])));

        var pipelinesFromBuildArtifactTask = await Task.WhenAll(tasks
            .Where(t => t != null &&
                        t.Enabled &&
                        t.TaskId.ToString() == rules.Last().TaskId &&
                        VerifyRuleInputs(t, rules.Last().Inputs))
            .Select(async t => await _pipelineEvaluator.GetPipelineAsync(organization, allBuildPipelines,
                t.Inputs["project"], t.Inputs["definition"])));

        return pipelinesFromPipelineArtifactTask
            .Concat(pipelinesFromBuildArtifactTask);
    }

    private async Task<IEnumerable<BuildDefinition>> GetLinkedMainframeCobolPipelinesAsync(ReleaseDefinition releaseDefinition)
    {
        var workFlowTasks = releaseDefinition.Environments
            .SelectMany(e => e.DeployPhases)
            .SelectMany(d => d.WorkflowTasks)
            .ToList();

        var dbbDeployTasks = workFlowTasks
            .Where(x => x.TaskId.ToString() == TaskContants.MainframeCobolConstants.DbbDeployTaskId
                        && x.Enabled);

        if (dbbDeployTasks == null || !dbbDeployTasks.Any())
        {
            return Enumerable.Empty<BuildDefinition>();
        }

        return await Task.WhenAll(dbbDeployTasks.Select(task => GetReferencedMainframeCobolPipeline(task.Inputs)));
    }
}