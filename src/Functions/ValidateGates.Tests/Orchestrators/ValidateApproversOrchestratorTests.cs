#nullable enable

using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Functions.ValidateGates.Activities;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Tests.Orchestrators;

public class ValidateApproversOrchestratorTests
{
    private readonly Mock<IDurableOrchestrationContext> _durableOrchestrationClient = new();
    private readonly Fixture _fixture = new();

    [Fact]
    public async Task ValidateApproversOrchestrator_RunAsync_GetInput_OneOfTheInputIsNull_ShouldRaiseNullReferenceException()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        const string projectId = "987987987987";
        var release = new Release();
        const string organization = "raboweb-test";
            
        var nullInputs = new ValidateApproversAzdoData(request, organization, null, projectId, null, null, release) { PlanId = null };

        _durableOrchestrationClient
            .Setup(x => x.GetInput<ValidateApproversAzdoData>())
            .Returns(nullInputs)
            .Verifiable();

        var sut = new ValidateApproversOrchestrator();

        // Act
        var actual = () => sut.RunAsync(_durableOrchestrationClient.Object);

        // Assert
        await actual.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task ValidateApproversOrchestrator_RunAsync_HappyFlow()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        const string projectId = "987987987987";
        var release = new Release { Id = 1234, Name = "ReleaseNameTest", ReleaseDefinition = new ReleaseDefinition { Id = "123455", Name = "releaseDefinitionName" } };
        const string organization = "raboweb-test";

        var azdoData = new ValidateApproversAzdoData(request, organization, null, projectId, null, null, release);
        var approvers = new ValidateApproversResult { DeterminedApprovalType = ApprovalType.PipelineApproval, Message = "approved" };

        var input = (azdoData.ProjectId, azdoData.Release, azdoData.Organization);
        var result = (azdoData, approvers.Message);
        var conclusion = (azdoData, true);

        _durableOrchestrationClient
            .Setup(x => x.GetInput<ValidateApproversAzdoData>())
            .Returns(azdoData)
            .Verifiable();
        _durableOrchestrationClient
            .Setup(x => x.CallActivityWithRetryAsync(nameof(SendTaskStartedActivity),
                It.IsAny<RetryOptions>(), azdoData))
            .Verifiable();
        _durableOrchestrationClient
            .Setup(x => x.CallActivityWithRetryAsync<ValidateApproversResult>(nameof(ValidateClassicApproversActivity),
                It.IsAny<RetryOptions>(), input))
            .ReturnsAsync(approvers)
            .Verifiable();
        _durableOrchestrationClient
            .Setup(x => x.CallActivityWithRetryAsync(nameof(AppendToTaskLogActivity),
                It.IsAny<RetryOptions>(), result))
            .Verifiable();
        _durableOrchestrationClient
            .Setup(x => x.CallActivityWithRetryAsync(nameof(SendTaskCompletedActivity),
                It.IsAny<RetryOptions>(), conclusion))
            .Verifiable();

        var sut = new ValidateApproversOrchestrator();
        
        // Act
        await sut.RunAsync(_durableOrchestrationClient.Object);
        
        // Assert
        _durableOrchestrationClient.VerifyAll();
    }

    [Fact]
    public async Task RunAsync_OrchestrationThrowsException_TaskCompletedEventIsSent()
    {
        // Arrange
        var azdoData = _fixture.Create<ValidateApproversAzdoData>();
                        
        _durableOrchestrationClient
            .Setup(x => x.GetInput<ValidateApproversAzdoData>())
            .Returns(azdoData)
            .Verifiable();

        _durableOrchestrationClient
            .Setup(x => x.CallActivityWithRetryAsync(nameof(SendTaskStartedActivity),
                It.IsAny<RetryOptions>(), azdoData))
            .ThrowsAsync(new Exception("Something went wrong"));

        var sut = new ValidateApproversOrchestrator();

        // Act
        await sut.RunAsync(_durableOrchestrationClient.Object);

        // Assert
        (ValidateApproversAzdoData, bool) taskCompletedData = (azdoData, false);
        (ValidateApproversAzdoData, string) appendToTaskLogData = (azdoData, "Something unexpected happened while validating 4-eyes approval.");

        _durableOrchestrationClient.Verify(x => x.CallActivityWithRetryAsync(nameof(SendTaskCompletedActivity),
            It.IsAny<RetryOptions>(), taskCompletedData));
        _durableOrchestrationClient.Verify(x => x.CallActivityWithRetryAsync(nameof(AppendToTaskLogActivity),
            It.IsAny<RetryOptions>(), appendToTaskLogData));
    }

    private static HttpRequestMessage CreateDefaultRequestWithHeaders()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", "raboweb-test");
        request.Headers.Add("HubName", "raboweb-HubName");
        request.Headers.Add("PlanId", "raboweb-PlanIdt");
        request.Headers.Add("JobId", "raboweb-JobId");
        request.Headers.Add("TaskInstanceId", "raboweb-TaskInstanceId");
        request.Headers.Add("AuthToken", "token");
        request.Headers.Add("ProjectId", "projectId");
        return request;
    }
}