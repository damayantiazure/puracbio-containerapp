#nullable enable

using AutoMapper;
using Microsoft.TeamFoundation.DistributedTask.WebApi;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.AzureDevOps;
using Rabobank.Compliancy.Infrastructure.Extensions;
using Rabobank.Compliancy.Infrastructure.InternalContracts;
using Rabobank.Compliancy.Infrastructure.Models;
using Rabobank.Compliancy.Infrastructure.Models.Yaml;
using Rabobank.Compliancy.Infrastructure.Parsers;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

namespace Rabobank.Compliancy.Infrastructure;


/// <inheritdoc/>
public class PipelineService : IPipelineService
{
    private const string _pipelineNotFoundError = "Could not find pipeline of type {0} with id {1} in project with id {2} for organization {3}";

    private readonly Dictionary<Guid, IList<Pipeline>> _cachedReleaseDefinitions = new();
    private readonly Dictionary<Guid, IList<Pipeline>> _cachedBuildDefinitions = new();
    private readonly Dictionary<Guid, IList<TaskGroup>> _cachedTaskGroups = new();

    private readonly Dictionary<string, Pipeline> _localPipelineCache = new();
    private readonly List<Project> _localProjectCache = new();

    private readonly IBuildRepository _buildDefinitionRepository;
    private readonly IReleaseRepository _releaseDefinitionRepository;
    private readonly ITaskGroupRepository _taskGroupRepository;
    private readonly IGateService _gateService;
    private readonly IPipelineRepository _pipelineRepository;
    private readonly IProjectService _projectService;
    private readonly IGitRepoService _gitRepoService;
    private readonly IMapper _mapper;

    [SuppressMessage("Sonar Code Smell",
        "S107: Constructor has 8 parameters, which is greater than the 7 authorized.",
        Justification = "Required by design.")]
    public PipelineService(IBuildRepository buildDefinitionRepository,
        IReleaseRepository releaseDefinitionRepository,
        IGateService gateService,
        IPipelineRepository pipelineRepository,
        IProjectService projectService,
        IGitRepoService gitRepoService,
        ITaskGroupRepository taskGroupRepository,
        IMapper mapper)
    {
        _buildDefinitionRepository = buildDefinitionRepository;
        _releaseDefinitionRepository = releaseDefinitionRepository;
        _gateService = gateService;
        _pipelineRepository = pipelineRepository;
        _projectService = projectService;
        _gitRepoService = gitRepoService;
        _taskGroupRepository = taskGroupRepository;
        _mapper = mapper;
    }

    public async Task<Pipeline> GetPipelineAsync<TPipeline>(Project project, int pipelineId, CancellationToken cancellationToken = default) where TPipeline : Pipeline
    {
        if (typeof(TPipeline) == typeof(AzdoBuildDefinitionPipeline))
        {
            var pipeline = await GetPipelineAsync(project, pipelineId, PipelineProcessType.DesignerBuild, cancellationToken);
            return _mapper.Map<AzdoBuildDefinitionPipeline>(pipeline);
        }

        if (typeof(TPipeline) == typeof(AzdoReleaseDefinitionPipeline))
        {
            var pipeline = await GetPipelineAsync(project, pipelineId, PipelineProcessType.DesignerRelease, cancellationToken);
            return _mapper.Map<AzdoReleaseDefinitionPipeline>(pipeline);
        }

        throw new ArgumentException($"Invalid pipeline type: {typeof(TPipeline)}");
    }

    /// <inheritdoc />
    public async Task<Pipeline> GetPipelineAsync(Project project, int pipelineId, PipelineProcessType definitionType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pipeline = definitionType switch
            {
                PipelineProcessType.DesignerBuild or PipelineProcessType.Yaml or PipelineProcessType.UnknownBuild =>
                    (await GetCachedMappedBuildDefinitionsForProject(project, cancellationToken))?
                    .FirstOrDefault(buildDefinition => buildDefinition.Id == pipelineId),
                PipelineProcessType.DesignerRelease =>
                    (await GetCachedMappedReleaseDefinitionsForProject(project, cancellationToken))?
                    .FirstOrDefault(releasePipeline => releasePipeline.Id == pipelineId),
                _ => throw new ArgumentException($"Invalid pipeline process type: {definitionType}", nameof(definitionType))
            };

            return pipeline ?? throw new SourceItemNotFoundException(_pipelineNotFoundError, definitionType, pipelineId,
                project.Id, project.Organization);
        }
        catch (HttpRequestException ex)
        {
            switch (ex.StatusCode)
            {
                case HttpStatusCode.NotFound:
                    throw new SourceItemNotFoundException(
                        string.Format(CultureInfo.InvariantCulture, _pipelineNotFoundError, definitionType, pipelineId,
                            project.Id, project.Organization),
                        ex);
                default:
                    throw;
            }
        }
    }

    /// <inheritdoc />
    public async Task<Pipeline> GetSinglePipelineForScanAsync(Project project, int pipelineId, PipelineProcessType definitionType, CancellationToken cancellationToken = default)
    {
        if (_localPipelineCache.ContainsKey($"{pipelineId}{project.Id}"))
        {
            return _localPipelineCache[$"{pipelineId}{project.Id}"];
        }

        if (_localProjectCache.TrueForAll(p => p.Id != project.Id))
        {
            _localProjectCache.Add(project);
        }

        var pipeline = await GetPipelineAsync(project, pipelineId, definitionType, cancellationToken);

        if (definitionType == PipelineProcessType.DesignerRelease)
        {
            var detailedPipeline =
                (await _releaseDefinitionRepository.GetReleaseDefinitionByIdAsync(project.Organization, project.Id, pipeline.Id, cancellationToken))
                ?.ToPipeline(project, await GetCachedTaskGroupsForProject(project, cancellationToken)) ?? throw new SourceItemNotFoundException();

            _localPipelineCache.Add($"{pipelineId}{project.Id}", detailedPipeline);

            return detailedPipeline;
        }

        await PopulateDefaultRunWithInformationFromYaml(pipeline, project, cancellationToken);
        pipeline.ConsumedResources = await GetConsumedPipelinesAsync(pipeline, cancellationToken);

        _localPipelineCache.Add($"{pipelineId}{project.Id}", pipeline);

        return pipeline;
    }

    #region Interact with YamlModel and detailed Release

    private async Task PopulateDefaultRunWithInformationFromYaml(Pipeline pipeline, Project project, CancellationToken cancellationToken)
    {
        var yamlPipelineModel = await GetYamlPipelineModel(pipeline, cancellationToken) ?? throw new InvalidOperationException("Yaml can not be null, something went wrong!");
        var stages = yamlPipelineModel.Stages?.ToArray() ?? Array.Empty<StageModel>();

        var usedEnvironmentNamesFromJobsInStages = yamlPipelineModel.Stages?.SelectMany(s => s.Jobs)
            .Where(j => j.Environment != null)
            .Select(j => j.Environment.Name);

        pipeline.DefaultRunContent ??= new PipelineBody();
        pipeline.DefaultRunContent.Stages = stages
            .Select(s => new Stage { Id = s.Stage, Name = s.DisplayName });

        pipeline.DefaultRunContent.Tasks = stages
            .SelectMany(s => s.Jobs)
            .SelectMany(j => j.GetAllSteps())
            .Select(x => new PipelineTask { Name = x.Task.StripNamespaceAndVersion(), Inputs = x.Inputs });

        pipeline.DefaultRunContent.Gates = await _gateService.GetGatesForBuildDefinitionAsync(project, pipeline.Id, usedEnvironmentNamesFromJobsInStages, cancellationToken);

        if (yamlPipelineModel.Resources?.Repositories != null)
        {
            await AddGitRepos(pipeline, yamlPipelineModel, cancellationToken);
        }

        await AddResourceFromPipelineAsync(pipeline, yamlPipelineModel, cancellationToken);
    }

    private async Task<YamlModel> GetYamlPipelineModel(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        var yaml = pipeline.DefinitionType == PipelineProcessType.Yaml
            ? await _pipelineRepository.GetPipelineYamlFromPreviewRunAsync(pipeline.Project.Organization, pipeline.Project.Id, pipeline.Id, cancellationToken)
            : await _buildDefinitionRepository.GetPipelineClassicBuildYaml(pipeline.Project.Organization, pipeline.Project.Id, pipeline.Id, cancellationToken);

        return YamlParser.ParseToYamlModel(yaml);
    }

    private async Task AddGitRepos(Pipeline pipeline, YamlModel yamlPipelineModel, CancellationToken cancellationToken = default)
    {
        var repositories = yamlPipelineModel.Resources?.Repositories;
        if (repositories == null)
        {
            return;
        }

        if (pipeline.DefaultRunContent == null)
        {
            throw new InvalidOperationException($"{nameof(pipeline.DefaultRunContent)} cannot be null.");
        }

        var gitRepositories = repositories.Where(r => r.Type.Equals("git", StringComparison.OrdinalIgnoreCase)
                                                      && !string.IsNullOrEmpty(r.Name));

        foreach (var gitRepo in gitRepositories)
        {
            pipeline.DefaultRunContent.Resources ??= new List<Domain.Compliancy.PipelineResource>();
            pipeline.DefaultRunContent.Resources = pipeline.DefaultRunContent.Resources.Concat(
                new[] { await GetGitRepoByRepositoryModel(pipeline, gitRepo, cancellationToken) });
        }
    }

    private async Task<GitRepo> GetGitRepoByRepositoryModel(Pipeline pipeline, RepositoryModel yamlRepo, CancellationToken cancellationToken)
    {
        var nameParts = yamlRepo.Name.Split('/');
        var repoName = nameParts[^1];
        var projectName = nameParts.Length == 1
            ? pipeline.Project.Name
            : nameParts[0];

        var project = await GetProjectFromLocalCache(pipeline.Project.Organization, projectName: projectName, cancellationToken: cancellationToken);

        var gitRepo = await _gitRepoService.GetGitRepoByNameAsync(project, repoName, cancellationToken);
        return gitRepo;
    }

    private async Task AddResourceFromPipelineAsync(Pipeline pipeline, YamlModel yamlModel, CancellationToken cancellationToken)
    {
        // check if there is a pipeline resource
        var pipelines = yamlModel.Resources?.Pipelines;
        if (pipelines == null)
        {
            return;
        }

        if (pipeline.DefaultRunContent == null)
        {
            throw new InvalidOperationException($"{nameof(pipeline.DefaultRunContent)} cannot be null.");
        }

        foreach (var pipelineResource in pipelines)
        {
            if (string.IsNullOrEmpty(pipelineResource.Source))
            {
                continue;
            }

            var pipelineName = pipelineResource.Source;

            var projectIdOrName = pipelineResource.Project.IsNotNullOrWhiteSpace()
                ? pipelineResource.Project
                : pipeline.Project.Id.ToString();

            var project = await GetProjectFromLocalCacheByName(pipeline.Project.Organization, projectIdOrName, cancellationToken);
            if (project == null)
            {
                continue;
            }

            var consumedPipeline = (await GetCachedMappedBuildDefinitionsForProject(project, cancellationToken))?
                .FirstOrDefault(x => x.Name.Equals(pipelineName));
            if (consumedPipeline == null)
            {
                continue;
            }

            pipeline.DefaultRunContent.Resources ??= new List<Domain.Compliancy.PipelineResource>();
            pipeline.DefaultRunContent.Resources = pipeline.DefaultRunContent.Resources.Append(consumedPipeline);
        }
    }

    #endregion

    #region Fetching consumed pipelines

    private async Task<IEnumerable<Pipeline>> GetConsumedPipelinesAsync(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        List<Pipeline> consumedPipelines = new();

        consumedPipelines.AddRange(await GetConsumedPipelinesFromTriggeredPipelineAsync(pipeline, cancellationToken));
        consumedPipelines.AddRange(await GetConsumedPipelinesFromDownloadTaskAsync(pipeline, cancellationToken));
        consumedPipelines.AddRange(await GetConsumedPipelinesFromMainFrameCobolTaskAsync(pipeline, cancellationToken));
        consumedPipelines.AddRange(await GetConsumedPipelinesFromResourcePipelineAsync(pipeline, cancellationToken));

        return consumedPipelines;
    }

    private async Task<IEnumerable<Pipeline>> GetConsumedPipelinesFromTriggeredPipelineAsync(
        Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        if (pipeline.DefaultRunContent?.Triggers == null || !pipeline.DefaultRunContent.Triggers.Any())
        {
            return Enumerable.Empty<Pipeline>();
        }

        // retrieve pipeline(s)
        List<Pipeline> returnList = new();
        foreach (var trigger in pipeline.DefaultRunContent.Triggers)
        {
            var projectOfConsumedPipeline = await GetProjectFromLocalCache(trigger.Organization,
                projectId: trigger.ProjectId, cancellationToken: cancellationToken);

            if (projectOfConsumedPipeline != null)
            {
                var consumedPipeline = await GetSinglePipelineForScanAsync(projectOfConsumedPipeline, trigger.Id,
                    PipelineProcessType.UnknownBuild, cancellationToken);
                returnList.Add(consumedPipeline);
            }
        }

        return returnList;
    }

    private async Task<IEnumerable<Pipeline>> GetConsumedPipelinesFromMainFrameCobolTaskAsync(Pipeline pipeline, CancellationToken cancellationToken)
    {
        var defaultRunContent = pipeline.DefaultRunContent;
        if (defaultRunContent?.Tasks == null || !defaultRunContent.Tasks.Any())
        {
            return Enumerable.Empty<Pipeline>();
        }

        var tasks = defaultRunContent.Tasks.ToList();
        List<Pipeline> returnList = new();
        foreach (var task in tasks.Where(DbbDeployTask.IsDbbDeployTask))
        {
            var deployTask = new DbbDeployTask(task);
            var projectId = deployTask.ReferencedProject;
            var pipelineId = deployTask.ReferencedPipelineId;
            if (projectId == null || pipelineId == null)
            {
                continue;
            }

            var projectOfConsumedPipeline = await GetProjectIfDifferent(pipeline.Project, projectId.Value, null, cancellationToken);
            if (projectOfConsumedPipeline != null)
            {
                var consumedPipeline = await GetSinglePipelineForScanAsync(projectOfConsumedPipeline, pipelineId.Value, PipelineProcessType.UnknownBuild, cancellationToken);
                returnList.Add(consumedPipeline);
            }
        }

        return returnList;
    }

    private async Task<IEnumerable<Pipeline>> GetConsumedPipelinesFromDownloadTaskAsync(Pipeline pipeline, CancellationToken cancellationToken)
    {
        var defaultRunContent = pipeline.DefaultRunContent;
        if (defaultRunContent?.Tasks == null || !defaultRunContent.Tasks.Any())
        {
            return Enumerable.Empty<Pipeline>();
        }

        List<Pipeline> returnList = new();
        foreach (var task in defaultRunContent.Tasks.Where(DownloadPipelineArtifactTask.IsDownloadPipelineArtifactTask))
        {
            var downloadTask = new DownloadPipelineArtifactTask(task);
            var consumedPipeline = await GetConsumedPipelineFromDownloadTaskAsync(pipeline.Project, downloadTask, cancellationToken);
            if (consumedPipeline == null)
            {
                continue;
            }

            returnList.Add(consumedPipeline);
        }

        return returnList;
    }

    private async Task<Pipeline?> GetConsumedPipelineFromDownloadTaskAsync(Project parentProject, DownloadPipelineArtifactTask downloadTask, CancellationToken cancellationToken = default)
    {
        var projectId = downloadTask.ReferencedProject;
        var pipelineId = downloadTask.ReferencedPipelineId;
        if (projectId == null || pipelineId == null)
        {
            return null;
        }

        var project = await GetProjectIfDifferent(parentProject, projectId.Value, cancellationToken: cancellationToken);
        if (project == null)
        {
            return null;
        }

        var pipeline = await GetSinglePipelineForScanAsync(project, pipelineId.Value, PipelineProcessType.UnknownBuild, cancellationToken);

        return pipeline;
    }

    private async Task<IEnumerable<Pipeline>> GetConsumedPipelinesFromResourcePipelineAsync(Pipeline pipeline, CancellationToken cancellationToken = default)
    {
        var resource = pipeline.DefaultRunContent?.Resources?.ToList();
        if (resource == null || !resource.Any())
        {
            return Enumerable.Empty<Pipeline>();
        }

        // get pipeline source and path
        List<Pipeline> returnList = new();
        foreach (var pipelineResource in resource.OfType<Pipeline>())
        {
            var consumedPipeline = await GetSinglePipelineForScanAsync(pipelineResource.Project, pipelineResource.Id, pipelineResource.DefinitionType, cancellationToken);
            returnList.Add(consumedPipeline);
        }

        return returnList;
    }

    #endregion

    #region Build / Release definitions and TaskGroups cache

    private async Task<IEnumerable<Pipeline>?> GetCachedMappedBuildDefinitionsForProject(Project project, CancellationToken cancellationToken = default)
    {
        if (_cachedBuildDefinitions.TryGetValue(project.Id, out var cachedBuildDefinitions))
        {
            return cachedBuildDefinitions;
        }

        var buildDefinitions =
            (await _buildDefinitionRepository.GetBuildDefinitionsByProjectAsync(project.Organization, project.Id,
                true, cancellationToken))?.Select(buildDefinition => buildDefinition.ToPipelineObject(project)).ToList();

        if (buildDefinitions != null)
        {
            _cachedBuildDefinitions.Add(project.Id, buildDefinitions);
        }

        return buildDefinitions;
    }

    private async Task<IEnumerable<Pipeline>?> GetCachedMappedReleaseDefinitionsForProject(Project project, CancellationToken cancellationToken = default)
    {
        if (_cachedReleaseDefinitions.TryGetValue(project.Id, out var cachedReleaseDefinitions))
        {
            return cachedReleaseDefinitions;
        }

        // Keep in mind we might add information to the pipelines later, so the collection has to be mutable (list instead of IEnumerable)
        var releaseDefinitions =
            (await _releaseDefinitionRepository.GetReleaseDefinitionsByProjectAsync(project.Organization, project.Id,
                cancellationToken))?.Select(releaseDefinition => releaseDefinition.ToPipeline(project)).ToList();

        if (releaseDefinitions != null)
        {
            _cachedReleaseDefinitions.Add(project.Id, releaseDefinitions);
        }

        return releaseDefinitions;
    }

    private async Task<IEnumerable<TaskGroup>?> GetCachedTaskGroupsForProject(
        Project project, CancellationToken cancellationToken = default)
    {
        if (_cachedTaskGroups.TryGetValue(project.Id, out var cachedTaskGroups))
        {
            return cachedTaskGroups;
        }

        // Keep in mind we might add information to the pipelines later, so the collection has to be mutable (list instead of IEnumerable)
        var taskGroups =
            (await _taskGroupRepository.GetTaskGroupsAsync(project.Organization, project.Id, cancellationToken))
            ?.ToList();

        if (taskGroups != null)
        {
            _cachedTaskGroups.Add(project.Id, taskGroups);
        }

        return taskGroups;
    }

    #endregion

    #region Project cache

    private async Task<Project?> GetProjectIfDifferent(Project parentProject, Guid projectId = default, string? projectName = null, CancellationToken cancellationToken = default) =>
        parentProject.Id == projectId || (projectId == Guid.Empty && parentProject.Name == projectName)
            ? parentProject : await GetProjectFromLocalCache(parentProject.Organization, projectId, projectName, cancellationToken);

    private async Task<Project?> GetProjectFromLocalCache(string organization, Guid projectId = default, string? projectName = null, CancellationToken cancellationToken = default)
    {
        if (projectId == Guid.Empty && projectName == null)
        {
            throw new InvalidOperationException($"Either {nameof(projectId)} or {nameof(projectName)} must be provided.");
        }

        if (projectId != Guid.Empty)
        {
            return await GetProjectFromLocalCacheById(organization, projectId, cancellationToken);
        }

        return await GetProjectFromLocalCacheByName(organization, projectName, cancellationToken);
    }

    private async Task<Project?> GetProjectFromLocalCacheById(string organization, Guid projectId, CancellationToken cancellationToken)
    {
        var project = _localProjectCache.Find(p => p.Id == projectId);
        if (project == null)
        {
            project = await _projectService.GetProjectByIdAsync(organization, projectId, cancellationToken);
            if (project != null)
            {
                _localProjectCache.Add(project);
            }
        }

        return project;
    }

    private async Task<Project?> GetProjectFromLocalCacheByName(string organization, string? projectName, CancellationToken cancellationToken)
    {
        var project = _localProjectCache.Find(p => p.Name == projectName);
        if (project == null)
        {
            project = await _projectService.GetProjectByNameAsync(organization, projectName, cancellationToken);
            if (project != null)
            {
                _localProjectCache.Add(project);
            }
        }

        return project;
    }

    #endregion
}