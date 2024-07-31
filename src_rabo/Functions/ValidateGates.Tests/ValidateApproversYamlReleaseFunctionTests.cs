#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Model;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Tests;

public class ValidateApproversYamlReleaseFunctionTests
{
    private readonly Mock<IAzdoRestClient> _azdoClient = new();
    private readonly Mock<IDurableOrchestrationClient> _durableOrchestrationClient = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    [Fact]
    public async Task RunAsync_HeaderIsMissing_ShouldRaiseArgumentNullException()
    {
        // Arrange
        var request = new HttpRequestMessage();
        var smokeTestOrganization = _fixture.Create<string>();
        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = () =>
            sut.RunAsync(request, It.IsAny<string>(), It.IsAny<string>(), smokeTestOrganization,
                _durableOrchestrationClient.Object);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_RunAsync_ValidateInput_ProjectIdIsNull()
    {
        // Arrange
        var projectId = It.IsAny<string>();
        const string runId = "1234";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organizationUri, false))
            .Returns(new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {nameof(projectId)} was not provided in the URL.")));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, null, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_RunAsync_ValidateInput_RunIsNull()
    {
        // Arrange
        const string projectId = "1234";
        var runId = It.IsAny<string>();
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organizationUri, false)).Returns(
            new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {nameof(runId)} was not provided in the URL.")));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_RunAsync_ValidateInput_RunIdNotNumber()
    {
        // Arrange
        const string projectId = "1234";
        const string runId = "1234d";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organizationUri, false)).Returns(
            new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"The runId: '{runId}' provided in the URL is invalid. It should only consist of numbers.")));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_RunAsync_ValidateInput_OrganizationUriCantBeParsed()
    {
        // Arrange
        const string projectId = "57272";
        const string runId = "12345";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", string.Empty);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, It.IsAny<string>(), false)).Returns(
            new BadRequestObjectResult(ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A 'PlanUrl' was not provided in the request header. " +
                $"PlanUrls can be provided by adding following to your request header:\n" +
                $"PlanUrl: $(system.CollectionUri)")));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_RunAsync_HappyFlow()
    {
        // Arrange
        const string projectId = "57272";
        const string runId = "12345";
        const string organization = "raboweb-test";
        var request = CreateDefaultRequestWithHeaders();

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organization, false))
            .Returns(new OkObjectResult(organization));
        _azdoClient.Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Timeline>>(), It.IsAny<string>()))
            .ReturnsAsync(_fixture.Create<Timeline>());

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        var actual = await sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_ShouldCallOrchestratorAsync()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        const string projectId = "57272";
        const string runId = "12345";
        const string organization = "raboweb-test";

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organization, false))
            .Returns(new OkObjectResult(organization));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        await sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        _durableOrchestrationClient
            .Verify(m => m.StartNewAsync(nameof(ValidateApproversOrchestrator), It.IsAny<ValidateApproversAzdoData>()),
                Times.Once);
    }

    [Fact]
    public async Task ValidateApproversYamlReleaseFunction_ShouldCallOrchestratorAsyncWithCorrectOrganization()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        var projectId = _fixture.Create<string>();
        var runId = _fixture.Create<string>();
        const string organization = "raboweb-test";
        var smokeTestOrganization = _fixture.Create<string>();

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organization, false))
            .Returns(new OkObjectResult(organization));

        var sut = new ValidateApproversYamlReleaseFunction(_validateInputServiceMock.Object, _azdoClient.Object,
            _loggingServiceMock.Object);

        // Act
        await sut.RunAsync(request, projectId, runId, smokeTestOrganization, _durableOrchestrationClient.Object);

        // Assert
        _durableOrchestrationClient
            .Verify(m => m.StartNewAsync(nameof(ValidateApproversOrchestrator), It.Is<ValidateApproversAzdoData>(x =>
                x.Organization == organization
                && x.SmoketestOrganization == smokeTestOrganization
                && x.RunId == runId
                && x.ProjectId == projectId
                && x.Release == null
                && x.StageId == "Production"
            )), Times.Once);
    }

    private static HttpRequestMessage CreateDefaultRequestWithHeaders()
    {
        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", "raboweb-test");
        request.Headers.Add("HubName", "raboweb-HubName");
        request.Headers.Add("PlanId", "537fdb7a-a601-4537-aa70-92645a2b5ce4");
        request.Headers.Add("JobId", "raboweb-JobId");
        request.Headers.Add("TaskInstanceId", "raboweb-TaskInstanceId");
        request.Headers.Add("AuthToken", "token");
        request.Headers.Add("ProjectId", "projectId");
        return request;
    }
}