using Flurl.Http;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Core.Rules.Helpers;

public class YamlEnvironmentHelper : IYamlEnvironmentHelper
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IPipelineRegistrationResolver _pipelineRegistrationResolver;

    public YamlEnvironmentHelper(
        IAzdoRestClient azdoClient,
        IPipelineRegistrationResolver pipelineRegistrationResolver)
    {
        _azdoClient = azdoClient;
        _pipelineRegistrationResolver = pipelineRegistrationResolver;
    }

    public async Task<IEnumerable<EnvironmentYaml>> GetProdEnvironmentsAsync(
        string organization, string projectId, BuildDefinition pipeline)
    {
        var registeredProdStages = await _pipelineRegistrationResolver.ResolveProductionStagesAsync(
            organization, projectId, pipeline.Id);

        if (pipeline.PipelineType == ItemTypes.DisabledYamlPipeline ||
            pipeline.PipelineType == ItemTypes.InvalidYamlPipeline)
        {
            throw new InvalidYamlPipelineException(ErrorMessages.InvalidYamlPipeline(
                "YAML pipeline could not be parsed. API response contained no FinalYaml."));
        }

        JToken yamlPipeline;
        if (string.IsNullOrEmpty(pipeline.YamlUsedInRun))
        {
            yamlPipeline = string.IsNullOrEmpty(pipeline.Yaml) // In case we use the yaml of the pipeline definition
                ? await GetYamlPipelineAsync(organization, projectId, pipeline.Id)
                : pipeline.Yaml.ToJson();
        }
        else
        {
            yamlPipeline = pipeline.YamlUsedInRun.ToJson(); // Otherwise we want the yaml used by that run
        }

        var prodStages = yamlPipeline
            .SelectTokens("stages[*]")
            .Where(s => registeredProdStages.Contains(GetTokenName(s["stage"]), StringComparer.OrdinalIgnoreCase));

        var allProjectEnvironments = await _azdoClient.GetAsync(Environments.All(projectId), organization);

        var prodEnvironments = prodStages
            .SelectMany(s => GetEnvironments(s, allProjectEnvironments))
            .Distinct();

        return prodEnvironments;
    }

    private async Task<JToken> GetYamlPipelineAsync(
        string organization, string projectId, string pipelineId)
    {
        try
        {
            var yamlPipelineResponse = await _azdoClient.PostAsync(YamlPipeline.Parse(projectId, pipelineId),
                new YamlPipeline.YamlPipelineRequest(), organization, true);

            if (yamlPipelineResponse?.FinalYaml == null)
            {
                throw new InvalidYamlPipelineException(ErrorMessages.InvalidYamlPipeline(
                    "YAML pipeline could not be parsed. API response contained no FinalYaml."));
            }

            return yamlPipelineResponse.FinalYaml.ToJson();
        }
        catch (FlurlHttpException e)
        {
            if (e?.Call?.HttpStatus == HttpStatusCode.BadRequest ||
                e?.Call?.HttpStatus == HttpStatusCode.NotFound ||
                e?.Call?.HttpStatus == HttpStatusCode.InternalServerError)
            {
                throw new InvalidYamlPipelineException(ErrorMessages.InvalidYamlPipeline(e?.Message));
            }

            throw;
        }
    }

    // A single stage can reference multiple environments at the same time
    private static IEnumerable<EnvironmentYaml> GetEnvironments(JToken stage, IEnumerable<EnvironmentYaml> allEnvironments)
    {
        var stageEnvironmentNames = stage.SelectTokens("jobs[*].environment")
            .Select(t => GetTokenName(t))
            .Where(name => name != null);

        if (stageEnvironmentNames == null || !stageEnvironmentNames.Any())
        {
            throw new EnvironmentNotFoundException(ErrorMessages.NoEnvironment());
        }

        // In case any runtime variable notations were used in the environmentNames we return these to the user as a comprehensive error
        var invalidEnvironmentNames = stageEnvironmentNames.Where(e => e.Contains("$("));

        if (invalidEnvironmentNames.Any())
        {
            throw new InvalidEnvironmentException(ErrorMessages.InvalidEnvironments(string.Join(", ", invalidEnvironmentNames)));
        }

        // Environment names can be of format 'Name.MachineName' for example "Production.lsrv9891", hence the split.
        // The name of an Environment itself cannot contain a dot (.), it is always used as a separator between components (VMs, clusters, etc) of an Environment
        // However, if an invalid Environment Name was provided, this may contain a dot, hence the split only happening here and not earlier in the process.
        var validEnvironmentNames = stageEnvironmentNames.Select(name => name.Split('.')[0]);

        var stageEnvironments = allEnvironments
            .Where(environment => validEnvironmentNames
                .Contains(environment.Name, System.StringComparer.InvariantCultureIgnoreCase));

        if (stageEnvironments == null || !stageEnvironments.Any())
        {
            throw new EnvironmentNotFoundException(ErrorMessages.NoEnvironment());
        }
        return stageEnvironments;
    }



    private static string GetTokenName(JToken token)
    {
        if (token == null)
        {
            return null;
        }

        var name = token.SelectToken("name") == null
            ? token.ToString()
            : token["name"]?.ToString();

        return string.IsNullOrWhiteSpace(name)
            ? null
            : name;
    }
}