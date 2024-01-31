using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Flurl.Http;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Model;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.AzdoClient.Response.Interfaces;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;

public class PipelineService : IPipelinesService
{
    private const int _parallelApiCalls = 200;
    private readonly SemaphoreSlim _semaphoreSlim = new(_parallelApiCalls);
    private const string _stagesSelector = "stages[*].stage";
    private const string _disabledStatus = "disabled";

    private readonly IAzdoRestClient _azdoClient;
    private readonly ConcurrentDictionary<string, IEnumerable<BuildDefinition>> _allYamlPipelines = new();
    private readonly ConcurrentDictionary<string, IEnumerable<BuildDefinition>> _classicBuildPipelines = new();
    private readonly ConcurrentDictionary<string, IEnumerable<ReleaseDefinition>> _classicReleasePipelines = new();

    public PipelineService(IAzdoRestClient azdoClient) =>
        _azdoClient = azdoClient;

    /// <summary>
    /// Gets all classic release pipelines for this project. Results are cached until the end of this HTTP request (the request to the function)
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="projectId"></param>
    /// <param name="pipelineRegistrations"></param>
    public async Task<IEnumerable<ReleaseDefinition>> GetClassicReleasePipelinesAsync(
        string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var key = $"{organization}-{projectId}";
        if (_classicReleasePipelines.ContainsKey(key) && _classicReleasePipelines[key] != null)
        {
            return _classicReleasePipelines[key];
        }

        try
        {
            await _semaphoreSlim.WaitAsync();
            var pipelineResult = await _azdoClient.GetAsync(ReleaseManagement.Definitions(projectId, "Artifacts,Environments"), organization);
            _classicReleasePipelines.TryAdd(key, pipelineResult);
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        AddPipelineRegistrations(_classicReleasePipelines[key], pipelineRegistrations, Constants.ItemTypes.ClassicReleasePipeline);

        return _classicReleasePipelines[key];
    }

    /// <summary>
    /// Gets all yaml pipelines for this project. Results are cached until the end of this HTTP request (the request to the function)
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="projectId"></param>
    /// <param name="pipelineRegistrations"></param>
    public async Task<IEnumerable<BuildDefinition>> GetAllYamlPipelinesAsync(
        string organization, string projectId, IEnumerable<PipelineRegistration> pipelineRegistrations)
    {
        var key = $"{organization}-{projectId}";
        if (_allYamlPipelines.ContainsKey(key) && _allYamlPipelines[key] != null)
        {
            return _allYamlPipelines[key];
        }

        try
        {
            await _semaphoreSlim.WaitAsync();
            var pipelineResult = await _azdoClient.GetAsync(Builds.BuildDefinitions(projectId, true, Constants.PipelineProcessType.YamlPipeline), organization);
            _allYamlPipelines.TryAdd(key, pipelineResult);
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        AddPipelineRegistrations(_allYamlPipelines[key], pipelineRegistrations, Constants.ItemTypes.YamlReleasePipeline);
        await Task.WhenAll(_allYamlPipelines[key].Select(
            async pipeline => await EnrichPipelineAsync(pipeline, organization))
        );
        return _allYamlPipelines[key];
    }

    /// <summary>
    /// Gets all classic build pipelines for this project. Results are cached until the end of this HTTP request (the request to the function)
    /// </summary>
    /// <param name="organization"></param>
    /// <param name="projectId"></param>
    public async Task<IEnumerable<BuildDefinition>> GetClassicBuildPipelinesAsync(string organization, string projectId)
    {
        var key = $"{organization}-{projectId}";
        if (_classicBuildPipelines.ContainsKey(key) && _classicBuildPipelines[key] != null)
        {
            return _classicBuildPipelines[key];
        }

        try
        {
            await _semaphoreSlim.WaitAsync();
            var pipelineResult = await _azdoClient.GetAsync(Builds.BuildDefinitions(projectId, true, Constants.PipelineProcessType.GuiPipeline), organization);

            _classicBuildPipelines.TryAdd(key, pipelineResult);
        }
        finally
        {
            _semaphoreSlim.Release();
        }

        foreach (var pipeline in _classicBuildPipelines[key])
            pipeline.PipelineType = Constants.ItemTypes.ClassicBuildPipeline;

        return _classicBuildPipelines[key];
    }

    public async Task EnrichPipelineAsync(BuildDefinition yamlPipeline, string organization)
    {
        // If the pipeline is disabled, we set the type to disabled and go to the next
        if (IsDisabled(yamlPipeline))
        {
            yamlPipeline.PipelineType = Constants.ItemTypes.DisabledYamlPipeline;
            return;
        }

        // If the pipeline is NOT disabled, we parse the pipeline (this means fetching the yaml from the Azdo API)
        yamlPipeline.Yaml = await ParseYamlPipelineAsync(organization, yamlPipeline);

        // If the yaml is invalid, we set the pipeline type to invalid and go to the next
        if (string.IsNullOrEmpty(yamlPipeline.Yaml))
        {
            yamlPipeline.PipelineType = Constants.ItemTypes.InvalidYamlPipeline;
            return;
        }

        // If the yaml is valid, we turn it into json to be able to parse stages
        var yamlAsJson = yamlPipeline.Yaml.ToJson();

        // If the pipeline has no stages, we set the pipeline type to stageless and go to the next
        if (!HasStages(yamlAsJson))
        {
            yamlPipeline.PipelineType = Constants.ItemTypes.StagelessYamlPipeline;
            return;
        }

        // If the pipeline has stages, we set the pipeline type to WithStages and convert the stages from Yaml to a list of Stage objects
        yamlPipeline.PipelineType = Constants.ItemTypes.YamlPipelineWithStages;
        yamlPipeline.Stages = GetStages(yamlAsJson);
    }

    private async Task<string> ParseYamlPipelineAsync(string organization, BuildDefinition yamlPipeline)
    {
        try
        {
            await _semaphoreSlim.WaitAsync();
            return (await _azdoClient.PostAsync(YamlPipeline.Parse(yamlPipeline.Project.Id, yamlPipeline.Id),
                new YamlPipeline.YamlPipelineRequest(), organization, true))?.FinalYaml ?? string.Empty;
        }
        catch (FlurlHttpException e)
        {
            if (e.Call?.HttpStatus == HttpStatusCode.BadRequest ||          //Pipeline invalid
                e.Call?.HttpStatus == HttpStatusCode.NotFound ||            //Pipeline not found
                e.Call?.HttpStatus == HttpStatusCode.InternalServerError)   //Pipeline resource not found
            {
                return string.Empty;
            }

            throw;
        }
        catch (SocketException e)
        {
            throw new ScanException(ErrorMessages.FinalYamlCouldNotBeRetrieved(yamlPipeline.Id, yamlPipeline.Name), e);
        }
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private static void AddPipelineRegistrations(IEnumerable<IRegisterableDefinition> definitions, IEnumerable<PipelineRegistration> pipelineRegistrations, string itemType)
    {
        if (pipelineRegistrations == null)
        {
            return;
        }

        foreach (var definition in definitions)
            definition.PipelineRegistrations = pipelineRegistrations.Where(registration =>
                registration.PipelineId == definition.Id && registration.PipelineType == itemType);
    }

    private static bool IsDisabled(BuildDefinition pipeline) => pipeline.QueueStatus == _disabledStatus;

    private static bool HasStages(JToken yaml) => yaml.SelectTokens(_stagesSelector).Any(s => s.ToString() != "__default");

    private static IEnumerable<Stage> GetStages(JToken yaml) =>
        yaml.SelectTokens(_stagesSelector).Select(yamlStage => new Stage { Id = yamlStage.ToString(), Name = yamlStage.ToString() });
}