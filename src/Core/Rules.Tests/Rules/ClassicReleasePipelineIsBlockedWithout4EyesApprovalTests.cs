using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Collections.Generic;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Core.Rules.Tests.Rules;

public class ClassicReleasePipelineIsBlockedWithout4EyesApprovalTests
{
    private readonly Mock<IPipelineRegistrationResolver> _pipelineRegistrationResolver = new();
    private readonly Mock<IAzdoRestClient> _client = new();
    private const string AzureFunctionTaskId = "537fdb7a-a601-4537-aa70-92645a2b5ce4";

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(false, true, false)]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    public async Task EvaluateAsync_EnabledAndDisabledGate(bool isGateEnabled, bool isTaskEnabled, bool expectedResult)
    {
        // Arrange
        var releasePipeline = new ReleaseDefinition
        {
            Id = "1",
            Environments = new[]
            {
                new ReleaseDefinitionEnvironment
                {
                    Id = 1,
                    PreDeploymentGates = new ReleaseDefinitionGatesStep {
                        GatesOptions = new ReleaseDefinitionGatesOptions
                        {
                            IsEnabled = isGateEnabled
                        },
                        Gates = new[] {
                            new ReleaseDefinitionGate {
                                Tasks = new[]
                                {
                                    new WorkflowTask {
                                        Enabled = isTaskEnabled,
                                        TaskId = new Guid(AzureFunctionTaskId),
                                        Inputs = new Dictionary<string, string>{
                                            ["function"] = "https://validategatesdev.azurewebsites.net/api/validate-classic-approvers/afd/333",
                                            ["waitForCompletion"] = "true"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        _pipelineRegistrationResolver
            .Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "1" });

        var ruleConfig = new RuleConfig { ValidateGatesHostName = "http://localhost" };

        // Act
        var rule = new ClassicReleasePipelineIsBlockedWithout4EyesApproval(null, _pipelineRegistrationResolver.Object, ruleConfig);
        var result = await rule.EvaluateAsync("test", "test", releasePipeline);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [InlineData(AzureFunctionTaskId, "https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/afd/333", "false", false)]
    [InlineData(AzureFunctionTaskId, "https://validategatesprd.azurewebsites.net/api/validate-classic-approvers/afd/333", "true", true)]

    // other task or wrong url
    [InlineData("05b07d7b-b004-4858-808a-9358e11e7659", "https://validategatesdev.azurewebsites.net/api/validate-gate/afd/333", "true", false)]
    [InlineData("05b07d7b-b004-4858-808a-9358e11e7659", "https://azdocompliancy.test.net/api/validate-gate/afd/333", "true", false)]
    [InlineData(AzureFunctionTaskId, "https://azdocompliancy.test.net/api/validate-gate/afd/333", "true", false)]
    public async Task EvaluateAsync_CheckGateCallsFunction(string taskGuid, string functionUrl, string waitForCompletion, bool expectedResult)
    {
        // Arrange
        var releasePipeline = new ReleaseDefinition
        {
            Id = "1",
            Environments = new[]
            {
                new ReleaseDefinitionEnvironment
                {
                    Id = 1,
                    PreDeploymentGates = new ReleaseDefinitionGatesStep {
                        GatesOptions = new ReleaseDefinitionGatesOptions
                        {
                            IsEnabled = true
                        },
                        Gates = new[] {
                            new ReleaseDefinitionGate {
                                Tasks = new[]
                                {
                                    new WorkflowTask {
                                        Enabled = true,
                                        TaskId = new Guid(taskGuid),
                                        Inputs = new Dictionary<string, string>{
                                            ["function"] = functionUrl,
                                            ["waitForCompletion"] = waitForCompletion
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        };

        _pipelineRegistrationResolver
            .Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "1" });

        var ruleConfig = new RuleConfig { ValidateGatesHostName = "http://localhost" };

        var rule = new ClassicReleasePipelineIsBlockedWithout4EyesApproval(null, _pipelineRegistrationResolver.Object, ruleConfig);

        // Act
        var result = await rule.EvaluateAsync("test", "test", releasePipeline);

        // Assert
        result.ShouldBe(expectedResult);
    }

    [Theory]
    [MemberData(nameof(GetValidReconcileTestCases))]
    public async Task ReconcileAsync_UpdateReleaseDefinitionWithGateTask(ReleaseDefinition releaseDefinition)
    {
        // Arrange
        _client
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<JObject>>(), It.IsAny<string>()))
            .ReturnsAsync(JObject.FromObject(releaseDefinition, new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

        _pipelineRegistrationResolver
            .Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new[] { "1" });

        var ruleConfig = new RuleConfig { ValidateGatesHostName = "http://localhost" };

        var rule = (IReconcile)new ClassicReleasePipelineIsBlockedWithout4EyesApproval(_client.Object, _pipelineRegistrationResolver.Object,
            ruleConfig);

        // Act
        await rule.ReconcileAsync("", "", "1");

        // Assert
        _client
            .Verify(m => m.PutAsync(It.IsAny<VsrmRequest<object>>(), It.Is<JObject>(
                    d => (bool)d.SelectTokens("environments[*].preDeploymentGates.gatesOptions.isEnabled").First() &&
                         d.SelectTokens("environments[*].preDeploymentGates.gates[*].tasks[?(@." +
                                        "taskId == '537fdb7a-a601-4537-aa70-92645a2b5ce4' && @.enabled == true)]").Any()),
                It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ReconcileAsync_WhenNoProductionItemsFoundThenNoAction()
    {
        // Arrange
        _client
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<JObject>>(), It.IsAny<string>()))
            .ReturnsAsync(JObject.FromObject(new ReleaseDefinition(), new JsonSerializer { ContractResolver = new CamelCasePropertyNamesContractResolver() }));

        _pipelineRegistrationResolver
            .Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new string[0]);

        var ruleConfig = new RuleConfig { ValidateGatesHostName = "http://localhost" };

        var rule = (IReconcile)new ClassicReleasePipelineIsBlockedWithout4EyesApproval(_client.Object, _pipelineRegistrationResolver.Object,
            ruleConfig);

        // Act
        await rule.ReconcileAsync("", "", "1");

        // Assert
        _client
            .Verify(m => m.PutAsync(It.IsAny<VsrmRequest<object>>(), It.IsAny<JObject>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ReconcileAsync_WhenNoReleaseDefinitionFoundThenNoAction()
    {
        // Arrange
        _client
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<JObject>>(), It.IsAny<string>()))
            .ReturnsAsync(default(JObject));

        _pipelineRegistrationResolver
            .Setup(m => m.ResolveProductionStagesAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(new string[0]);

        var ruleConfig = new RuleConfig { ValidateGatesHostName = "http://localhost" };

        var rule = (IReconcile)new ClassicReleasePipelineIsBlockedWithout4EyesApproval(_client.Object, _pipelineRegistrationResolver.Object,
            ruleConfig);

        // Act
        await rule.ReconcileAsync("", "", "1");

        // Assert
        _client
            .Verify(m => m.PutAsync(It.IsAny<VsrmRequest<object>>(), It.IsAny<JObject>(), It.IsAny<string>()), Times.Never);
    }

    public static IEnumerable<object[]> GetValidReconcileTestCases()
    {
        var releaseDefinitionWithNoExistingGate = new object[]
        {
            new ReleaseDefinition
            {
                Environments = new[] { new ReleaseDefinitionEnvironment { Id = 1 } }
            }
        };

        yield return releaseDefinitionWithNoExistingGate;

        var releaseDefinitionWithExistingGate = new object[]
        {
            new ReleaseDefinition
            {
                Environments = new[]
                {
                    new ReleaseDefinitionEnvironment
                    {
                        Id = 1,
                        PreDeploymentGates = new ReleaseDefinitionGatesStep
                        {
                            Gates = new [] { new ReleaseDefinitionGate
                                {
                                    Tasks = new[] {
                                        new WorkflowTask
                                        {
                                            Enabled = true,
                                            TaskId = Guid.NewGuid(),
                                            Name = "Dummy task"
                                        }}
                                }
                            },
                            GatesOptions = new ReleaseDefinitionGatesOptions
                            {
                                IsEnabled = true
                            }
                        }
                    }
                }
            }
        };

        yield return releaseDefinitionWithExistingGate;

        var releaseDefinitionWithDisabledFunctionTask = new object[]
        {
            new ReleaseDefinition
            {
                Environments = new[]
                {
                    new ReleaseDefinitionEnvironment
                    {
                        Id = 1,
                        PreDeploymentGates = new ReleaseDefinitionGatesStep
                        {
                            Gates = new [] { new ReleaseDefinitionGate
                                {
                                    Tasks = new[] {
                                        new WorkflowTask
                                        {
                                            Enabled = false,
                                            TaskId = new Guid(AzureFunctionTaskId),
                                            Name = "Validate artifact is produced by master/main branch."
                                        }}
                                }
                            },
                            GatesOptions = new ReleaseDefinitionGatesOptions
                            {
                                IsEnabled = true
                            }
                        }
                    }
                }
            }
        };

        yield return releaseDefinitionWithDisabledFunctionTask;
    }
}