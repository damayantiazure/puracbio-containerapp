using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Extensions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Helpers;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

using System.Threading.Tasks;

public class ScanItemsService : IScanItemsService
{
    private const int ParallelApiCalls = 20;

    private readonly IEnumerable<IProjectRule> _projectRules;
    private readonly IEnumerable<IRepositoryRule> _repositoryRules;
    private readonly IEnumerable<IBuildPipelineRule> _buildPipelineRules;
    private readonly IEnumerable<IYamlReleasePipelineRule> _yamlReleasePipelineRules;
    private readonly IEnumerable<IClassicReleasePipelineRule> _classicReleasePipelineRules;
    private readonly ComplianceConfig _config;
    private readonly IRepositoryService _repoService;

    public ScanItemsService(
        IEnumerable<IProjectRule> projectRules,
        IEnumerable<IRepositoryRule> repositoryRules,
        IEnumerable<IBuildPipelineRule> buildPipelineRules,
        IEnumerable<IYamlReleasePipelineRule> yamlReleasePipelineRules,
        IEnumerable<IClassicReleasePipelineRule> classicReleasePipelineRules,
        ComplianceConfig config,
        IRepositoryService repoService)
    {
        _projectRules = projectRules;
        _repositoryRules = repositoryRules;
        _buildPipelineRules = buildPipelineRules;
        _yamlReleasePipelineRules = yamlReleasePipelineRules;
        _classicReleasePipelineRules = classicReleasePipelineRules;
        _config = config;
        _repoService = repoService;
    }

    public async Task<IList<EvaluatedRule>> ScanProjectAsync(
        string organization, Project project, string ciIdentifier) =>
        await Task.WhenAll(_projectRules
            .Select(async rule => new EvaluatedRule(rule)
            {
                Status = await rule.EvaluateAsync(organization, project.Id),
                Item = new Item
                {
                    Id = project.Id,
                    Name = project.Name,
                    Type = ItemTypes.Project
                },
                Reconcile = CreateUrl.ReconcileFromRule(_config, organization,
                    project.Id, project.Id, rule as IProjectReconcile),
                RescanUrl = CreateUrl.ItemRescanUrl(_config, organization, project.Id,
                    rule.Name, project.Id),
                RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(
                    _config, organization, project.Id, ciIdentifier, rule.Name, project.Id),
                DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(
                    _config, organization, project.Id, ciIdentifier, rule.Name, project.Id)
            }));

    public async Task<IEnumerable<EvaluatedRule>> ScanRepositoriesAsync(
        string organization, Project project, IEnumerable<Repository> repositories, string ciIdentifier)
    {
        if (!repositories.Any())
        {
            return _repositoryRules
                .Select(rule => CreateDummyItem(organization, project, ciIdentifier, rule));
        }

        return await Task.WhenAll(repositories
            .SelectMany(item => _repositoryRules
                .Select(async (rule, i) =>
                {
                    var semaphoreSlim = new SemaphoreSlim(ParallelApiCalls);
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        var itemProjectId = string.IsNullOrEmpty(item.Project?.Id)
                            ? await _repoService.GetProjectIdByNameAsync(organization, project, item)
                            : item.Project.Id;

                        bool isSameProject = itemProjectId == project.Id;

                        var foreignProjectId = isSameProject
                            ? null
                            : itemProjectId;

                        return new EvaluatedRule(rule)
                        {
                            Status = await rule.EvaluateAsync(organization, itemProjectId, item.Id),
                            Item = new Item
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Type = ItemTypes.Repository,
                                Link = (await _repoService.GetUrlAsync(organization, project, item)).AbsoluteUri,
                                ProjectId = itemProjectId
                            },
                            Reconcile = isSameProject
                                ? CreateUrl.ReconcileFromRule(_config, organization, project.Id, item.Id, rule as IReconcile)
                                : null,
                            RescanUrl = CreateUrl.ItemRescanUrl(_config, organization, project.Id, rule.Name, item.Id,
                                foreignProjectId),
                            RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(_config, organization, project.Id, ciIdentifier,
                                rule.Name, item.Id, foreignProjectId),
                            DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(_config, organization, project.Id, ciIdentifier,
                                rule.Name, item.Id, foreignProjectId)
                        };
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                })));
    }

    public async Task<IEnumerable<EvaluatedRule>> ScanBuildPipelinesAsync(
        string organization, Project project, IEnumerable<BuildDefinition> buildPipelines, string ciIdentifier,
        IEnumerable<RuleProfile> ruleProfilesForCi)
    {
        // Dummy items are added if no buildpipeline is found for current CI to make it possible
        // to add a deviation for a certain rule.
        // We also only want to show the rules which are relevant for the current CI
        if (!buildPipelines.Any())
        {
            var rulesForCi = ruleProfilesForCi.SelectMany(r => r.Rules);
            var rules = _buildPipelineRules.IntersectBy(rulesForCi, r => r.Name);

            return rules
                .Select(rule => CreateDummyItem(organization, project, ciIdentifier, rule));
        }

        var evaluatedRules = new List<Task<EvaluatedRule>>();

        foreach (var buildPipeline in buildPipelines)
        {
            var ruleProfile = buildPipeline.ProfileToApply;

            evaluatedRules.AddRange(_buildPipelineRules.GetAllByRuleProfile(ruleProfile)
                .Select(async (rule, i) =>
                    {
                        var semaphoreSlim = new SemaphoreSlim(ParallelApiCalls);
                        await semaphoreSlim.WaitAsync();

                        try
                        {
                            bool isSameProject = buildPipeline.Project.Id == project.Id;

                            var foreignProjectId = isSameProject
                                ? null
                                : buildPipeline.Project.Id;

                            return new EvaluatedRule(rule)
                            {
                                Status = await rule.EvaluateAsync(organization, buildPipeline.Project.Id, buildPipeline),
                                Item = CreateDefaultItemByPipeline(buildPipeline, ItemTypes.BuildPipeline),
                                Reconcile = isSameProject
                                    ? CreateUrl.ReconcileFromRule(_config, organization, project.Id, buildPipeline.Id, rule as IReconcile)
                                    : null,
                                RescanUrl = CreateUrl.ItemRescanUrl(_config, organization, project.Id, rule.Name, buildPipeline.Id,
                                    foreignProjectId),
                                RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(_config, organization, project.Id, ciIdentifier,
                                    rule.Name, buildPipeline.Id, foreignProjectId),
                                DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(_config, organization, project.Id, ciIdentifier,
                                    rule.Name, buildPipeline.Id, foreignProjectId)
                            };
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    }
                ));
        }
        return await Task.WhenAll(evaluatedRules);
    }

    public async Task<IEnumerable<EvaluatedRule>> ScanYamlReleasePipelinesAsync(
        string organization, Project project, IEnumerable<BuildDefinition> yamlReleasePipelines, string ciIdentifier)
    {
        var evaluatedRules = new List<Task<EvaluatedRule>>();

        foreach (var pipeline in yamlReleasePipelines)
        {
            var profile = pipeline.ProfileToApply;

            evaluatedRules.AddRange(_yamlReleasePipelineRules.GetAllByRuleProfile(profile)
                .Select(async (rule, i) =>
                {
                    var semaphoreSlim = new SemaphoreSlim(ParallelApiCalls);
                    await semaphoreSlim.WaitAsync();

                    try
                    {
                        return new EvaluatedRule(rule)
                        {
                            Status = !IsFourEyesPrincipleAndNoRegisteredProdStage((IRule)rule, pipeline) && await rule.EvaluateAsync(organization, project.Id, pipeline),
                            Item = CreateDefaultItemByPipeline(pipeline, ItemTypes.YamlReleasePipeline),
                            Reconcile = IsFourEyesPrincipleAndNoRegisteredProdStage((IRule)rule, pipeline)
                                ? null
                                : CreateUrl.ReconcileFromRule(_config, organization, project.Id, pipeline.Id, rule as IReconcile),
                            RescanUrl = CreateUrl.ItemRescanUrl(_config, organization, project.Id, rule.Name, pipeline.Id),
                            RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(_config, organization, project.Id, ciIdentifier, rule.Name, pipeline.Id),
                            DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(_config, organization, project.Id, ciIdentifier, rule.Name, pipeline.Id)
                        };
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }));
        }

        return await Task.WhenAll(evaluatedRules);
    }

    public async Task<IEnumerable<EvaluatedRule>> ScanClassicReleasePipelinesAsync(
        string organization, Project project, IEnumerable<ReleaseDefinition> classicReleasePipelines, string ciIdentifier)
    {
        var evaluatedRules = new List<Task<EvaluatedRule>>();

        foreach (var pipeline in classicReleasePipelines)
        {
            RuleProfile profile = pipeline.ProfileToApply;
            var filteredRules = _classicReleasePipelineRules.GetAllByRuleProfile(profile);
            evaluatedRules.AddRange(
                filteredRules.Select(async (rule, i) =>
                {
                    var semaphoreSlim = new SemaphoreSlim(ParallelApiCalls);
                    await semaphoreSlim.WaitAsync();
                    try
                    {
                        return new EvaluatedRule(rule)
                        {
                            Status = !IsFourEyesPrincipleAndNoRegisteredProdStage(rule, pipeline) && await rule.EvaluateAsync(organization, project.Id, pipeline),
                            Item = new Item
                            {
                                Id = pipeline.Id,
                                Name = pipeline.Name,
                                Type = ItemTypes.ClassicReleasePipeline,
                                Link = pipeline.Links.Web.Href.AbsoluteUri,
                                ProjectId = project.Id
                            },
                            Reconcile = IsFourEyesPrincipleAndNoRegisteredProdStage(rule, pipeline) ? null :
                                CreateUrl.ReconcileFromRule(_config, organization, project.Id, pipeline.Id, rule as IReconcile),
                            RescanUrl = CreateUrl.ItemRescanUrl(_config, organization, project.Id,
                                rule.Name, pipeline.Id),
                            RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(_config, organization,
                                project.Id, ciIdentifier, rule.Name, pipeline.Id),
                            DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(_config, organization,
                                project.Id, ciIdentifier, rule.Name, pipeline.Id)
                        };
                    }
                    finally
                    {
                        semaphoreSlim.Release();
                    }
                }));
        }

        return await Task.WhenAll(evaluatedRules);
    }

    private EvaluatedRule CreateDummyItem(string organization, Project project, string ciIdentifier, IRule rule) =>
        new(rule)
        {
            Status = false,
            Item = new Item
            {
                Id = ItemTypes.Dummy,
                Type = ItemTypes.Dummy,
                ProjectId = project.Id
            },
            RescanUrl = CreateUrl.ItemRescanUrl(_config, organization,
                project.Id, rule.Name, ItemTypes.Dummy),
            RegisterDeviationUrl = CreateUrl.RegisterDeviationUrl(_config, organization,
                project.Id, ciIdentifier, rule.Name, ItemTypes.Dummy),
            DeleteDeviationUrl = CreateUrl.DeleteDeviationUrl(_config, organization,
                project.Id, ciIdentifier, rule.Name, ItemTypes.Dummy)
        };

    private static Item CreateDefaultItemByPipeline(BuildDefinition pipeline, string itemType) =>
        new()
        {
            Id = pipeline.Id,
            Name = pipeline.Name,
            Type = itemType,
            Link = pipeline.Links.Web.Href.AbsoluteUri,
            ProjectId = pipeline.Project.Id
        };

    private static bool IsFourEyesPrincipleAndNoRegisteredProdStage(IRule rule, BuildDefinition buildDefinition) =>
        buildDefinition.PipelineRegistrations.All(registration => registration.StageId == null) &&
        rule.Principles.Any(p => p == BluePrintPrinciples.FourEyes);

    private static bool IsFourEyesPrincipleAndNoRegisteredProdStage(IRule rule, ReleaseDefinition releaseDefinition) =>
        releaseDefinition.PipelineRegistrations.All(registration => registration.StageId == null) &&
        rule.Principles.Any(p => p == BluePrintPrinciples.FourEyes);
}