using Flurl;
using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Helpers;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Environment = System.Environment;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class YamlReleasePipelineIsBlockedWithout4EyesApproval : ReconcilableBuildPipelineRule, IYamlReleasePipelineRule, IReconcile
{
    private readonly IAzdoRestClient _client;
    private readonly IYamlEnvironmentHelper _yamlEnvironmentHelper;
    private readonly RuleConfig _ruleConfig;

    public YamlReleasePipelineIsBlockedWithout4EyesApproval(
        IAzdoRestClient client,
        IYamlEnvironmentHelper yamlEnvironmentHelper,
        RuleConfig ruleConfig)
        : base(client)
    {
        _client = client;
        _yamlEnvironmentHelper = yamlEnvironmentHelper;
        _ruleConfig = ruleConfig;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.YamlReleasePipelineIsBlockedWithout4EyesApproval;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "YAML production deployment is blocked without 4-eyes approval";

    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/NYoVEg";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.FourEyes };


    public override async Task<bool> EvaluateAsync(
        string organization, string projectId, BuildDefinition buildPipeline)
    {
        try
        {
            ValidateInput(organization, projectId, buildPipeline);

            var prodEnvironments = await _yamlEnvironmentHelper
                .GetProdEnvironmentsAsync(organization, projectId, buildPipeline);
            if (!prodEnvironments.Any())
            {
                return false;
            }

            foreach (var prodEnvironment in prodEnvironments)
            {
                var checks = await GetCheckConfigurations(projectId, organization, prodEnvironment.Id);
                if (!checks.Any())
                {
                    return false;
                }

                if (!HasCheck(checks, CheckExcludingRetryInterval))
                {
                    return false;
                }

            }

            return true;
        }
        catch (Exception e) when (
            e is InvalidClassicPipelineException ||
            e is InvalidYamlPipelineException ||
            e is EnvironmentNotFoundException ||
            e is InvalidEnvironmentException)
        {
            return false;
        }


    }


    public override async Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        ValidateInput(organization, projectId, itemId);

        var prodEnvironments = await _yamlEnvironmentHelper
            .GetProdEnvironmentsAsync(organization, projectId, new BuildDefinition { Id = itemId });
        if (!prodEnvironments.Any())
        {
            return;
        }

        await Task.WhenAll(prodEnvironments
            .Select(async environment => await DeleteAll4EyesChecksAndCreateSingleNewAsync(organization, projectId, environment)));
    }

    private async Task DeleteAll4EyesChecksAndCreateSingleNewAsync(string organization, string projectId, EnvironmentYaml environment)
    {
        var checks = await GetCheckConfigurations(projectId, organization, environment.Id);
        await (DeleteAll4EyesChecks(projectId, organization, checks));
        await _client.PostAsync(Environments.CreateCheck(projectId), Environments.CreateCheckBody(
            _ruleConfig.ValidateGatesHostName, environment.Name, environment.Id), organization);
    }

    private async Task DeleteAll4EyesChecks(string projectId, string organization, JToken[] checks)
    {
        var all4EyesChecks = GetAll4EyesChecks(checks);
        var tasks = all4EyesChecks.Select(GetConfigId) // Get all ID's
            .Select(configId => DeleteCheck(projectId, configId, organization)); // Create delete tasks from them
        await Task.WhenAll(tasks);
    }

    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "For each production stage (as stored in ITSM) an environment check will be added " +
        "that calls a function app that validates whether the artifact that is deployed " +
        "to the production stage conforms to the 4 eye principle. (Environment checks can " +
        "be found under 'Environments/Approvals and checks')",
        "Notes: when using variables in environment names they should be early bound, e.g. ${{variables.variableName}}, "+
        "and all checks with display name '4-eyes principle check' and type 'invoke azure function' will be deleted " +
        "and a new compliant check will be created.",
        "See: https://docs.microsoft.com/en-us/azure/devops/pipelines/process/variables"
    };

    private static bool HasCheck(JToken[] checks, Func<JToken, bool> settingsCheckMethod) =>
        checks.Any() && checks.Any(settingsCheckMethod);

    private async Task<JToken[]> GetCheckConfigurations(string projectId, string organization, int environmentId)
    {
        var jObject = await _client.GetAsync(Environments.Checks(projectId, environmentId), organization);
        var result = jObject?.SelectTokens(
            "fps.dataProviders.data.['ms.vss-pipelinechecks.checks-data-provider']" +
            ".checkConfigurationDataList[*].checkConfiguration").ToArray();
        return result;
    }

    private static string GetConfigId(JToken setting) =>
        setting.SelectToken("id")?.ToString();

    private static IEnumerable<JToken> GetAll4EyesChecks(IEnumerable<JToken> settings) =>
        settings.Where(Is4EyesCheck);

    private static bool Is4EyesCheck(JToken setting) =>
        setting.SelectToken("settings.definitionRef.name")?.ToString() == "AzureFunction"
        && setting.SelectToken("settings.displayName")?.ToString() == "4-eyes principle check";

    private static bool CheckExcludingRetryInterval(JToken setting) =>
        setting.SelectToken("settings.definitionRef.name")?.ToString() == "AzureFunction"
            && setting.SelectToken("settings.inputs.method")?.ToString() == "POST"
            && setting.SelectToken("settings.inputs.waitForCompletion")?.ToString() == "true"
            && UrlMatchesGate(setting.SelectToken("settings.inputs.function")?.ToString());


    private Task DeleteCheck(string projectId, string configId, string organization) =>
        _client.DeleteAsync(Environments.DeleteCheck(projectId, configId), organization);

    private static void ValidateInput(string organization, string projectId, string itemId)
    {
        if (organization == null)
        {
            throw new ArgumentNullException(nameof(organization));
        }

        if (projectId == null)
        {
            throw new ArgumentNullException(nameof(projectId));
        }

        if (itemId == null)
        {
            throw new ArgumentNullException(nameof(itemId));
        }
    }

    private static void ValidateInput(string organization, string projectId, BuildDefinition buildPipeline)
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
    }

    private static bool UrlMatchesGate(Url url) =>
        new Regex(@"^https:\/\/validategates((dev)|(prd))\.azurewebsites\.net\/api\/?validate-yaml-approvers\/.+\/.+")
            .IsMatch(url);

    public virtual string GetEnvironmentVariable(string variableName) =>
        Environment.GetEnvironmentVariable(variableName, EnvironmentVariableTarget.Process)
        ?? throw new InvalidOperationException(
            $"Please provide a valid value for environment variable '{variableName}'");
}