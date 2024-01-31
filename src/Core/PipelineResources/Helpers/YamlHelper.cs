using Flurl.Http;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.PipelineResources.Extensions;
using Rabobank.Compliancy.Core.PipelineResources.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public class YamlHelper : IYamlHelper

{
    private readonly IMemoryCache _cache;
    private readonly IAzdoRestClient _azdoRestClient;
    private const double DefaultCacheExpirationInSeconds = 25;

    public YamlHelper(IMemoryCache cache, IAzdoRestClient azdoRestClient)
    {
        _cache = cache;
        _azdoRestClient = azdoRestClient;
    }

    public async Task<IEnumerable<PipelineTaskInputs>> GetPipelineTasksAsync(string organization, string projectId, BuildDefinition buildPipeline)
    {
        JToken yamlPipeline;
        if (string.IsNullOrEmpty(buildPipeline.YamlUsedInRun))
        {
            yamlPipeline = string.IsNullOrEmpty(buildPipeline.Yaml) // In case we use the yaml of the pipeline definition
                ? await GetYamlPipelineAsync(organization, projectId, buildPipeline)
                : buildPipeline.Yaml.ToJson();
        }
        else
        {
            yamlPipeline = buildPipeline.YamlUsedInRun.ToJson(); // Otherwise we want the yaml used by that run
        }

        if (yamlPipeline == null)
        {
            return Enumerable.Empty<PipelineTaskInputs>();
        }

        var pipelineInputs = new List<PipelineTaskInputs>();

        foreach (var step in GetSteps(yamlPipeline))
        {
            var taskName = step["task"];

            if (taskName == null)
            {
                continue;
            }

            pipelineInputs.Add(
                new PipelineTaskInputs
                {
                    FullTaskName = taskName.ToString(),
                    Inputs = step["inputs"].ToInputsDictionary(),
                    Enabled = step.SelectToken("enabled", false)?.ToString()?.ToUpperInvariant() != "FALSE"
                }
            );
        }
        return pipelineInputs;
    }

    private async Task<JToken> GetYamlPipelineAsync(string organization, string projectId, BuildDefinition buildPipeline)
    {
        try
        {
            var endpoint = YamlPipeline.Parse(projectId, buildPipeline.Id);

            var response = await _cache.GetOrCreateAsync(endpoint.Url().ToString(),
                entry =>
                {
                    entry.SlidingExpiration = TimeSpan.FromSeconds(DefaultCacheExpirationInSeconds);
                    return _azdoRestClient.PostAsync(endpoint, new YamlPipeline.YamlPipelineRequest(),
                        organization, true);
                });

            if (response?.FinalYaml == null)
            {
                return null;
            }

            return response?.FinalYaml.ToJson();
        }
        catch (FlurlHttpException e)
        {
            if (e?.Call?.HttpStatus == HttpStatusCode.BadRequest ||
                e?.Call?.HttpStatus == HttpStatusCode.NotFound ||
                e?.Call?.HttpStatus == HttpStatusCode.InternalServerError)
            {
                return null;
            }

            throw;
        }
    }

    private static IEnumerable<JToken> GetSteps(JToken yamlPipeline) =>
        yamlPipeline.SelectTokens("..steps[*]");
}