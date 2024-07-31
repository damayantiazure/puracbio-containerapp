using Flurl.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Rabobank.Compliancy.Core.Rules.Exceptions;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Domain.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Rules;

public class ClassicReleasePipelineIsBlockedWithout4EyesApproval : ReconcilableClassicReleasePipelineRule, IClassicReleasePipelineRule, IReconcile
{
    private const string AzureFunctionTaskId = "537fdb7a-a601-4537-aa70-92645a2b5ce4";
    private const string FunctionUrlKey = "function";
    private const string WaitForCompletionKey = "waitForCompletion";
    private const string DeploymentGateName = "Validate 4 eyes principle";
    private const int DefaultGateTimeoutInMinutes = 6;
    private const int DefaultSamplingIntervalInMinutes = 5;
    private readonly Regex functionUrl = new Regex(@"^https:\/\/validategates((dev)|(prd))\.azurewebsites\.net\/api\/?validate-classic-approvers\/.+\/.+");
    private readonly IAzdoRestClient _client;
    private readonly IPipelineRegistrationResolver _productionItemsResolver;
    private readonly RuleConfig _ruleConfig;

    private static readonly JsonSerializer serializer = new() { ContractResolver = new CamelCasePropertyNamesContractResolver() };

    public ClassicReleasePipelineIsBlockedWithout4EyesApproval(
        IAzdoRestClient client,
        IPipelineRegistrationResolver productionItemsResolver,
        RuleConfig ruleConfig) : base(client)
    {
        _client = client;
        _productionItemsResolver = productionItemsResolver;
        _ruleConfig = ruleConfig;
    }
    [ExcludeFromCodeCoverage]
    string IRule.Name => RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval;
    [ExcludeFromCodeCoverage]
    string IReconcile.Name => RuleNames.ClassicReleasePipelineIsBlockedWithout4EyesApproval;
    [ExcludeFromCodeCoverage]
    string IRule.Description => "Classic production deployment is blocked without 4-eyes approval";
    [ExcludeFromCodeCoverage]
    string IRule.Link => "https://confluence.dev.rabobank.nl/x/vNCMEQ";
    [ExcludeFromCodeCoverage]
    BluePrintPrinciple[] IRule.Principles => new[] { BluePrintPrinciples.FourEyes };
    [ExcludeFromCodeCoverage]
    string[] IReconcile.Impact => new[]
    {
        "For each production stage (as stored in ITSM) a deployment gate will be added that calls a function app that validates " +
        "whether the artifact that is deployed to the production stage conforms to the 4 eye principle. " +
        "(Deployment gates can be found under 'Pre-deployment conditions/Gates/Deployment gates')"
    };

    public override Task<bool> EvaluateAsync(string organization, string projectId,
        ReleaseDefinition releasePipeline)
    {
        if (releasePipeline == null)
        {
            throw new ArgumentNullException(nameof(releasePipeline));
        }

        return EvaluateInternalAsync(organization, projectId, releasePipeline);
    }

    private async Task<bool> EvaluateInternalAsync(string organization, string projectId, ReleaseDefinition releasePipeline)
    {

        var productionStages = await _productionItemsResolver.ResolveProductionStagesAsync(
            organization, projectId, releasePipeline.Id);

        if (!productionStages.Any())
        {
            return false;
        }

        return releasePipeline
            .Environments
            .Where(e => productionStages.Contains(e.Id.ToString()))
            .All(e => e.PreDeploymentGates?.GatesOptions?.IsEnabled == true &&
                      e.PreDeploymentGates.Gates.SelectMany(g => g.Tasks)
                          .Any(t => IsValidateGateTask(t) && t.Enabled));
    }

    public async override Task ReconcileAsync(string organization, string projectId, string itemId)
    {
        var productionStages = (await _productionItemsResolver.ResolveProductionStagesAsync(
            organization, projectId, itemId)).ToList();
        if (!productionStages.Any())
        {
            return;
        }

        var definition = await _client.GetAsync(
            ReleaseManagement.Definition(projectId, itemId).AsJson(), organization);

        foreach (var stage in productionStages)
        {
            if (!int.TryParse(stage, out var stageId))
            {
                continue;
            }

            var environment = definition.SelectTokens($"environments[?(@.id == {stageId})]")
                .FirstOrDefault();
            if (environment == null)
            {
                continue;
            }

            var preDeploymentGates = environment.SelectToken("preDeploymentGates");

            if (preDeploymentGates == null || !preDeploymentGates.Any())
            {
                preDeploymentGates = JToken.FromObject(CreatePredeploymentGates(), serializer);
                environment["preDeploymentGates"] = preDeploymentGates;
            }

            if (preDeploymentGates["gatesOptions"].Type == JTokenType.Null)
            {
                preDeploymentGates["gatesOptions"] =
                    JToken.FromObject(CreateReleaseDefinitionGatesOptions(), serializer);
            }

            var gates = UpdateGates(preDeploymentGates);
            preDeploymentGates["gates"] = JArray.FromObject(gates, serializer);
            preDeploymentGates["gatesOptions"]["isEnabled"] = true;
        }

        try
        {
            await _client.PutAsync(
                new VsrmRequest<object>($"{projectId}/_apis/release/definitions/{itemId}",
                    new Dictionary<string, object> { { "api-version", "5.1" } }),
                definition, organization);
        }
        catch (FlurlHttpException e)
        {
            throw new InvalidClassicPipelineException(ErrorMessages.InvalidClassicPipeline(e.Message));
        }
    }

    private List<JToken> UpdateGates(JToken preDeploymentGates) =>
        GetGates(preDeploymentGates).Append(CreateGate(serializer)).ToList();

    private JToken CreateGate(JsonSerializer serializer) =>
        JToken.FromObject(new ReleaseDefinitionGate
        {
            Tasks = new[] { CreateValidateGateTask() }
        }, serializer);

    private static List<JToken> GetGates(JToken preDeploymentGates) =>
        preDeploymentGates.SelectTokens("gates[*].tasks[?(@.name)]")
            .Where(x => x["name"].ToString() != DeploymentGateName)
            .ToList();

    private bool IsValidateGateTask(WorkflowTask t) =>
        t.TaskId.ToString() == AzureFunctionTaskId &&
        t.Inputs != null &&
        t.Inputs.ContainsKey(FunctionUrlKey) && CorrectConfiguration(t);

    private bool CorrectConfiguration(WorkflowTask t) =>
        functionUrl.IsMatch(t.Inputs[FunctionUrlKey]) &&
        t.Inputs.ContainsKey(WaitForCompletionKey) &&
        t.Inputs[WaitForCompletionKey] == "true";

    private WorkflowTask CreateValidateGateTask()
    {
        return new WorkflowTask
        {
            TaskId = new Guid(AzureFunctionTaskId),
            Name = DeploymentGateName,
            Enabled = true,
            Inputs = new Dictionary<string, string>
            {
                ["function"] = $"https://{_ruleConfig.ValidateGatesHostName}/api/validate-classic-approvers/" +
                               $"$(system.TeamProjectId)/$(Release.ReleaseId)",
                ["key"] = $"DUMMY",
                ["waitForCompletion"] = "true"
            },
            Version = "1.*",
            DefinitionType = "task"
        };
    }

    private static ReleaseDefinitionGatesStep CreatePredeploymentGates() =>
        new ReleaseDefinitionGatesStep
        {
            GatesOptions = CreateReleaseDefinitionGatesOptions(),
            Gates = Array.Empty<ReleaseDefinitionGate>()
        };

    private static ReleaseDefinitionGatesOptions CreateReleaseDefinitionGatesOptions() =>
        new ReleaseDefinitionGatesOptions
        {
            Timeout = DefaultGateTimeoutInMinutes,
            SamplingInterval = DefaultSamplingIntervalInMinutes
        };
}