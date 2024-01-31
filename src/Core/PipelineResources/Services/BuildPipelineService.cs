using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Domain.Constants;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Requests = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Services;

public class BuildPipelineService : PipelineServiceBase, IBuildPipelineService
{
    private const string CheckoutGitPrefix = "git://";
    private const string PipelineResourcesSelector = "resources.pipelines[*]";
    private const string SourceSelector = "source";
    private const string ProjectSelector = "project";
    private const string RespositoryResourcesSelector = "resources.repositories[*]";
    private const string TypeSelector = "type";
    private const string NameSelector = "name";
    private const string RepositorySelector = "repository";

    private readonly IPipelineHasTaskRule[] _rules =
    {
        new PipelineHasTaskRule("61f2a582-95ae-4948-b34d-a1b3c4f6a737")
        {
            TaskName = "DownloadPipelineArtifact",
            Inputs = new Dictionary<string, string>{{ "source", "specific" }}
        },
        new PipelineHasTaskRule("a433f589-fce1-4460-9ee6-44a624aeb1fb")
        {
            TaskName = "DownloadBuildArtifacts",
            Inputs = new Dictionary<string, string>{{ "buildType", "specific" }}
        },
        // YAML task with argument aliases
        new PipelineHasTaskRule("61f2a582-95ae-4948-b34d-a1b3c4f6a737")
        {
            TaskName = "DownloadPipelineArtifact",
            Inputs = new Dictionary<string, string>{{ "buildType", "specific" }}
        },
        // This is the 'checkout' step. Under the hood, when parsing the yaml, the 'checkout' step is a task.
        new PipelineHasTaskRule("6d15af64-176c-496d-b583-fd2ae21d4df4")
        {
            TaskName = "6d15af64-176c-496d-b583-fd2ae21d4df4"
        }
    };

    private readonly IAzdoRestClient _azdoClient;
    private readonly IYamlHelper _yamlHelper;
    private readonly PipelineEvaluatorFactory _pipelineEvaluatorFactory;

    public BuildPipelineService(IAzdoRestClient azdoClient, IMemoryCache cache, IYamlHelper yamlHelper) : base(azdoClient)
    {
        _azdoClient = azdoClient;
        _yamlHelper = yamlHelper;
        _pipelineEvaluatorFactory = new PipelineEvaluatorFactory(azdoClient, cache, yamlHelper);
    }

    public async Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, BuildDefinition yamlReleasePipeline, IEnumerable<BuildDefinition> allBuildPipelines = null)
    {
        var results = new ConcurrentBag<BuildDefinition>();
        await GetAllPipelinesAsync(organization, allBuildPipelines, yamlReleasePipeline, results);
        var profile = GetRuleProfileForYamlRelease(yamlReleasePipeline.PipelineRegistrations);
        yamlReleasePipeline.ProfileToApply = profile;

        foreach (var buildDefinition in results)
        {
            buildDefinition.ProfileToApply = profile;
        }

        return results;
    }

    public async Task<IEnumerable<BuildDefinition>> GetLinkedPipelinesAsync(
        string organization, IEnumerable<BuildDefinition> buildPipelines, IEnumerable<BuildDefinition> allBuildPipelines = null)
    {
        var results = new ConcurrentBag<BuildDefinition>();

        await StartNextIterationAsync(organization, allBuildPipelines, results, buildPipelines);

        return results;
    }

    private async Task GetLinkedMainframeCobolPipelinesForYamlAsync(BuildDefinition buildDefinition,
        string organization, string projectId, ConcurrentBag<BuildDefinition> handledPipelines)
    {
        var pipelineTasks = await _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildDefinition);
        var deployTasks = pipelineTasks
            .Where(t => t.FullTaskName.Contains(TaskContants.MainframeCobolConstants.DbbDeployTaskName));

        foreach (var deployTask in deployTasks)
        {
            var task = await GetReferencedMainframeCobolPipeline(deployTask.Inputs);
            if (task != null)
            {
                handledPipelines.Add(task);
            }
        }
    }

    public async Task<IEnumerable<Repository>> GetLinkedRepositoriesAsync(
        string organization, IEnumerable<BuildDefinition> buildPipelines)
    {
        var repositories = GetRepositories(buildPipelines);

        var resourceRepositories = (await Task.WhenAll(buildPipelines
                .Select(async d => await GetRepositoriesAsync(organization, d))))
            .SelectMany(r => r);

        return repositories
            .Concat(resourceRepositories)
            .Where(r => r != null)
            .GroupBy(r => r.Id)
            .Select(r => r.First());
    }

    private static IEnumerable<Repository> GetRepositories(IEnumerable<BuildDefinition> buildPipelines) =>
        buildPipelines
            .Where(x => x.Repository.Type == RepositoryTypes.TfsGit)
            .Select(x => x.Repository)
            .GroupBy(r => r.Url.AbsoluteUri)
            .Select(r => r.First());

    private async Task<IEnumerable<Repository>> GetRepositoriesAsync(string organization, BuildDefinition pipeline)
    {
        if (string.IsNullOrEmpty(pipeline.Yaml) && string.IsNullOrEmpty(pipeline.YamlUsedInRun))
        {
            return new List<Repository>();
        }

        var yaml = pipeline.UsedYaml.ToJson();

        var resourceRepositories = (await Task.WhenAll(yaml
            .SelectTokens(RespositoryResourcesSelector)
            .Select(async t => await GetResourceRepositoryAsync(organization, pipeline.Project.Id, t))));

        var checkedOutRepos = (await Task.WhenAll(yaml
            .SelectTokens("..steps[*]")
            .Where(s => StepContainsCheckout(s, _rules.Last().TaskName))
            .Select(async s => await GetRepositoryFromCheckoutStepAsync(organization, pipeline.Project.Id, s["inputs"]))));

        return resourceRepositories
            .Concat(checkedOutRepos);
    }

    private async Task<Repository> GetResourceRepositoryAsync(string organization, string projectId, JToken token)
    {
        var type = token.SelectToken(TypeSelector, false)?.ToString();
        if (string.IsNullOrEmpty(type) || type != RepositoryTypes.Git)
        {
            return null;
        }

        var name = token.SelectToken(NameSelector, false)?.ToString();
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        var parts = name.Split('/');
        var linkedRepositoryName = parts.Last();
        var linkedProject = parts.Length == 1
            ? projectId
            : parts.First();

        // Fetch repositories
        return await _azdoClient.GetAsync(Requests.Repository.Repo(linkedProject, linkedRepositoryName),
            organization);
    }

    private static bool StepContainsCheckout(JToken step, string taskName)
    {
        var task = step["task"];
        return task != null &&
               ContainsTaskName(task.ToString(), taskName) &&
               step.SelectToken("condition", false)?.ToString()?.ToUpperInvariant() != "FALSE";
    }

    private static bool ContainsTaskName(string fullTaskName, string name)
    {
        var taskName = fullTaskName.Split('@').First();
        return taskName == name;
    }

    private async Task<Repository> GetRepositoryFromCheckoutStepAsync(string organization, string projectId, JToken input)
    {
        var repository = input.SelectToken(RepositorySelector, false)?.ToString();

        if (string.IsNullOrEmpty(repository) || !repository.StartsWith(CheckoutGitPrefix))
        {
            return null;
        }

        var gitUrl = repository.Split('@').First();

        // Some projects use pipelines with a checkout step and build the repository url dynamically
        // based on some variables. If those variables are no provided in you end up with a url like: 
        // git:///@refs/heads/master
        var gitCheck2 = new Regex("^git:\\/\\/[^\\\\\\/]+\\/?[^\\\\\\/]+$");
        if (!gitCheck2.IsMatch(gitUrl))
        {
            throw new InvalidOperationException($"Git url: {repository} is not in a valid format");
        }
        var parts = gitUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var linkedRepositoryName = parts.Last();
        var linkedProject = parts.Length == 2
            ? projectId
            : parts[^2];

        return await _azdoClient.GetAsync(Requests.Repository.Repo(linkedProject, linkedRepositoryName),
            organization);
    }

    private async Task GetAllPipelinesAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        BuildDefinition pipeline, ConcurrentBag<BuildDefinition> handledPipelines)
    {
        var enrichedPipeline = await EnrichPipelineAsync(organization, pipeline);

        await GetPipelinesFromTriggersAsync(organization, allBuildPipelines, enrichedPipeline, handledPipelines);

        await GetPipelinesFromDownloadTasksAsync(organization, allBuildPipelines, enrichedPipeline, handledPipelines);

        await GetPipelinesFromResourcesAsync(organization, allBuildPipelines, enrichedPipeline, handledPipelines);

        await GetLinkedMainframeCobolPipelinesForYamlAsync(pipeline, organization, pipeline.Project.Id, handledPipelines);
    }

    private async Task GetPipelinesFromTriggersAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        BuildDefinition pipeline, ConcurrentBag<BuildDefinition> handledPipelines)
    {
        if (pipeline.Triggers == null ||
            !pipeline.Triggers.Any(t => t.TriggerType == "buildCompletion"))
        {
            return;
        }

        var linkedPipelines = (await Task.WhenAll(pipeline.Triggers
                .Where(x => !IsPipelineHandled(handledPipelines, x.Definition))
                .Select(async x => await GetPipelineAsync(organization, allBuildPipelines, x.Definition.Project.Id, x.Definition.Id))))
            .Distinct()
            .ToArray();

        await StartNextIterationAsync(organization, allBuildPipelines, handledPipelines, linkedPipelines);
    }

    private async Task GetPipelinesFromDownloadTasksAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        BuildDefinition pipeline, ConcurrentBag<BuildDefinition> handledPipelines)
    {
        var linkedPipelines = (await _pipelineEvaluatorFactory.GetPipelinesAsync(
                organization, allBuildPipelines, pipeline.Project.Id, pipeline, _rules))
            .Where(linkedPipeline => !IsPipelineHandled(handledPipelines, linkedPipeline))
            .Distinct()
            .ToArray();

        if (!linkedPipelines.Any())
        {
            return;
        }

        await StartNextIterationAsync(organization, allBuildPipelines, handledPipelines, linkedPipelines);
    }

    private async Task GetPipelinesFromResourcesAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        BuildDefinition pipeline, ConcurrentBag<BuildDefinition> handledPipelines)
    {
        if (string.IsNullOrEmpty(pipeline.Yaml) && string.IsNullOrEmpty(pipeline.YamlUsedInRun))
        {
            return;
        }

        var yaml = pipeline.UsedYaml.ToJson();
        var pipelineTokens = yaml.SelectTokens(PipelineResourcesSelector);
        if (!pipelineTokens.Any())
        {
            return;
        }

        var linkedPipelines = (await Task.WhenAll(pipelineTokens
                .Select(async pipelineToken => await GetResourcePipelineAsync(organization, allBuildPipelines, pipeline.Project.Id, pipelineToken))))
            .Distinct()
            .Where(pipeline => !IsPipelineHandled(handledPipelines, pipeline))
            .ToArray();

        await StartNextIterationAsync(organization, allBuildPipelines, handledPipelines, linkedPipelines);
    }

    private async Task<BuildDefinition> GetPipelineAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        string projectId, string itemId)
    {
        var pipeline = allBuildPipelines?.FirstOrDefault(d => d.Project.Id == projectId && d.Id == itemId);

        return pipeline ?? await _azdoClient.GetAsync(Requests.Builds.BuildDefinition(projectId, itemId), organization);
    }

    private static bool IsPipelineHandled(ConcurrentBag<BuildDefinition> handledPipelines,
        BuildDefinition pipeline) =>
        pipeline == null || handledPipelines.Any(h => h.Id == pipeline.Id && h.Project.Id == pipeline.Project.Id);

    private static bool IsPipelineHandled(ConcurrentBag<BuildDefinition> handledPipelines,
        Definition pipeline) =>
        pipeline == null || handledPipelines.Any(h => h.Id == pipeline.Id && h.Project.Id == pipeline.Project.Id);

    private async Task<BuildDefinition> EnrichPipelineAsync(
        string organization, BuildDefinition pipeline)
    {
        if (pipeline.Process.Type != PipelineProcessType.YamlPipeline)
        {
            return pipeline;
        }

        if (!pipeline.IsValidInput())
        {
            return pipeline;
        }

        try
        {
            var yamlResponse = await _azdoClient.PostAsync(Requests.YamlPipeline.Parse(
                    pipeline.Project.Id, pipeline.Id),
                new Requests.YamlPipeline.YamlPipelineRequest(), organization, true);
            pipeline.Yaml = yamlResponse.FinalYaml;
            return pipeline;
        }
        catch (FlurlHttpException e)
        {
            if (e?.Call?.HttpStatus == HttpStatusCode.BadRequest ||         //Pipeline invalid
                e?.Call?.HttpStatus == HttpStatusCode.NotFound ||           //Pipeline not found
                e?.Call?.HttpStatus == HttpStatusCode.InternalServerError)  //Pipeline resource not found
            {
                return pipeline;
            }
            throw;
        }
    }

    private async Task<BuildDefinition> GetResourcePipelineAsync(string organization,
        IEnumerable<BuildDefinition> allBuildPipelines, string projectId, JToken token)
    {
        var source = token.SelectToken(SourceSelector, false)?.ToString();
        if (string.IsNullOrEmpty(source))
        {
            return null;
        }

        var parts = source.Split('/');
        var linkedPipelineName = parts.Last();
        // In a project there can be pipelines with the same name. Therefore filtering on name AND path is required.
        var linkedPipelinePath = parts.Length == 1
            ? null
            : $"\\{string.Join(@"\", parts.Take(parts.Length - 1))}";

        var linkedPipelineProjectId = string.IsNullOrEmpty(token.SelectToken(ProjectSelector, false)?.ToString())
            ? projectId
            : token.SelectToken(ProjectSelector, false)?.ToString();

        if (allBuildPipelines == null || linkedPipelineProjectId != projectId)
        {
            var allPipelinesInProject = await _azdoClient.GetAsync(Requests.Builds.BuildDefinitions(
                linkedPipelineProjectId, true), organization);

            return GetResourcePipelineByNameAndPath(allPipelinesInProject, linkedPipelineName, linkedPipelinePath);
        }

        return GetResourcePipelineByNameAndPath(allBuildPipelines, linkedPipelineName, linkedPipelinePath);
    }

    private static BuildDefinition GetResourcePipelineByNameAndPath(
        IEnumerable<BuildDefinition> allPipelines, string pipelineName, string pipelinePath) =>
        allPipelines
            .SingleOrDefault(x => x.Name.Equals(pipelineName, StringComparison.InvariantCultureIgnoreCase) &&
                                  (string.IsNullOrEmpty(pipelinePath) ||
                                   pipelinePath.Equals(x.Path, StringComparison.InvariantCultureIgnoreCase)));

    private async Task StartNextIterationAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines,
        ConcurrentBag<BuildDefinition> handledPipelines, IEnumerable<BuildDefinition> linkedPipelines)
    {
        foreach (var pipeline in linkedPipelines)
        {
            handledPipelines.Add(pipeline);
        }

        await Task.WhenAll(linkedPipelines
            .Select(async pipeline => await GetAllPipelinesAsync(organization, allBuildPipelines, pipeline, handledPipelines)));
    }
}