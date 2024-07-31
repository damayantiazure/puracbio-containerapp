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
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.PipelineResources.Helpers;

public class YamlPipelineEvaluator : IPipelineEvaluator
{
    private readonly IAzdoRestClient _azdoClient;
    private readonly IYamlHelper _yamlHelper;
    private const string PipelineId = @"^\d+$";

    public YamlPipelineEvaluator(IAzdoRestClient client, IYamlHelper yamlHelper)
    {
        _azdoClient = client;
        _yamlHelper = yamlHelper;
    }

    public async Task<bool> EvaluateAsync(string organization, string projectId,
        BuildDefinition buildPipeline, IPipelineHasTaskRule rule)
    {
        ValidateInput(organization, projectId, buildPipeline, rule);
        var tasks = await _yamlHelper.GetPipelineTasksAsync(organization, projectId, buildPipeline);

        if (!tasks.Any())
        {
            return false;
        }

        return HasPipelineSpecifiedTaskAndInputs(tasks, rule);
    }

    private static void ValidateInput(string organization, string projectId, BuildDefinition buildPipeline, IPipelineHasTaskRule rule)
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

        if (buildPipeline.Process.YamlFilename == null)
        {
            throw new ArgumentOutOfRangeException(nameof(buildPipeline));
        }

        if (rule == null)
        {
            throw new ArgumentNullException(nameof(rule));
        }
    }

    private static bool HasPipelineSpecifiedTaskAndInputs
        (IEnumerable<PipelineTaskInputs> taskInputs, IPipelineHasTaskRule rule)
    {
        var tasks = taskInputs
            .Where(t => t.HasTaskNameOrIdAndIsEnabled(rule.TaskName) || t.HasTaskNameOrIdAndIsEnabled(rule.TaskId));

        if (!tasks.Any())
        {
            return false;
        }

        // Task is found but rule has no inputs defined to check for, so return true
        if (rule.Inputs == null || !rule.Inputs.Any())
        {
            return true;
        }

        // Check inputs specified for rule
        foreach (var task in tasks)
        {
            var result = VerifyRuleInputs(rule, task.Inputs);
            if (result)
            {
                return true;
            }
        }

        return false;
    }

    public static bool ContainsTaskName(string fullTaskName, string name)
    {
        var taskNameWithPrefix = fullTaskName.Split('@')[0];
        var taskName = taskNameWithPrefix.Split('.').Last();
        return taskName == name;
    }

    public static bool VerifyRuleInputs(IPipelineHasTaskRule pipelineHasTaskRule, Dictionary<string, string> pipelineInputs)
    {
        var ruleInputs = pipelineHasTaskRule.Inputs;
        if (ruleInputs == null)
        {
            return false;
        }

        foreach (var (ruleInputKey, ruleInputValue) in ruleInputs)
        {
            var inputValue = pipelineInputs?.FirstOrDefault(i => i.Key
                .Equals(ruleInputKey, StringComparison.OrdinalIgnoreCase)).Value;

            if (string.IsNullOrEmpty(inputValue))
            {
                return false;
            }

            // Only check input values if ignoreinputvalues is false
            if (!pipelineHasTaskRule.IgnoreInputValues)
            {
                if (IsBool(ruleInputValue))
                {
                    return ruleInputValue.Equals(inputValue, StringComparison.InvariantCultureIgnoreCase);
                }

                if (inputValue != ruleInputValue)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private static bool PipelineHasVariables(JToken pipelineInputs)
    {
        const string VariablePrefix = "$(";
        string item = RetrieveArgumentName(pipelineInputs);

        //variables cannot be resolved at the moment so we skip them
        if (pipelineInputs["project"].ToString().StartsWith(VariablePrefix) ||
            pipelineInputs[item].ToString().StartsWith(VariablePrefix))
        {
            return true;
        }

        return false;
    }

    private static bool IsBool(string boolString) =>
        boolString.Equals(bool.TrueString, StringComparison.InvariantCultureIgnoreCase) ||
        boolString.Equals(bool.FalseString, StringComparison.InvariantCultureIgnoreCase);

    private static bool StepContainsTask(JToken step, string taskName)
    {
        var task = step["task"];
        return task != null &&
               ContainsTaskName(task.ToString(), taskName) &&
               step.SelectToken("enabled", false)?.ToString()?.ToUpperInvariant() != "FALSE";
    }

    private static IEnumerable<JToken> GetSteps(JToken yamlPipeline) =>
        yamlPipeline.SelectTokens("..steps[*]");

    public async Task<IEnumerable<BuildDefinition>> GetPipelinesAsync(string organization,
        IEnumerable<BuildDefinition> allBuildPipelines, string projectId, BuildDefinition pipeline,
        IPipelineHasTaskRule[] rules)
    {
        if (string.IsNullOrEmpty(pipeline.Yaml) && string.IsNullOrEmpty(pipeline.YamlUsedInRun))
        {
            return new List<BuildDefinition>();
        }

        var yaml = pipeline.UsedYaml.ToJson();
        return await GetPipelinesFromStepsAsync(GetSteps(yaml), organization, allBuildPipelines, rules);
    }

    private async Task<IEnumerable<BuildDefinition>> GetPipelinesFromStepsAsync(IEnumerable<JToken> steps,
        string organization, IEnumerable<BuildDefinition> allBuildPipelines, IPipelineHasTaskRule[] rules)
    {
        var pipelinesFromPipelineArtifactTask = await Task.WhenAll(steps
            .Where(s => StepContainsTask(s, rules.First().TaskName) &&
                        (VerifyRuleInputs(rules.First(), s["inputs"].ToInputsDictionary()) ||
                         VerifyRuleInputs(rules[2], s["inputs"].ToInputsDictionary())) &&
                        !PipelineHasVariables(s["inputs"]))
            .Select(async s => await GetPipelineAsync(organization, allBuildPipelines, s["inputs"])));

        var pipelinesFromBuildArtifactTask = await Task.WhenAll(steps
            .Where(s => StepContainsTask(s, rules[1].TaskName) &&
                        VerifyRuleInputs(rules[1], s["inputs"].ToInputsDictionary()) &&
                        !PipelineHasVariables(s["inputs"]))
            .Select(async s => await GetPipelineAsync(organization, allBuildPipelines, s["inputs"])));

        return pipelinesFromPipelineArtifactTask
            .Concat(pipelinesFromBuildArtifactTask);
    }

    private async Task<BuildDefinition> GetPipelineAsync(string organization, IEnumerable<BuildDefinition> allBuildPipelines, JToken input)
    {
        string item = RetrieveArgumentName(input);

        // Input value for argument is 'ItemId'. Therefore, compare with 'definition.Id'
        if (new Regex(PipelineId).IsMatch(input[item].ToString()))
        {
            var result = allBuildPipelines?.FirstOrDefault(d =>
                IsFromSameProject(d, input["project"].ToString()) && d.Id == input[item].ToString());

            return result ?? await _azdoClient.GetAsync(Builds.BuildDefinition(
                input["project"].ToString(), input[item].ToString()), organization);
        }

        // Input value for argument is 'ItemName'. Therefore, compare with 'definition.Name'
        var found = allBuildPipelines?.FirstOrDefault(d =>
            IsFromSameProject(d, input["project"].ToString()) && d.Name == input[item].ToString());

        if (found != null)
        {
            return found;
        }

        var allPipelinesInProject = (await _azdoClient.GetAsync(Builds.BuildDefinitions(input["project"].ToString(), true), organization));

        return allPipelinesInProject.SingleOrDefault(x => x.Name.Equals(input[item].ToString(), StringComparison.InvariantCultureIgnoreCase));
    }

    private static bool IsFromSameProject(BuildDefinition pipeline, string projectName) =>
        pipeline.Project.Id == projectName || pipeline.Project.Name == projectName;

    private static string RetrieveArgumentName(JToken input) =>
        input.SelectToken("pipeline") != null ? "pipeline" : "definition";
}