#nullable enable

using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.InputValidation.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ValidateGates.Model;
using Rabobank.Compliancy.Functions.ValidateGates.Orchestrators;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Shouldly;
using System;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ValidateGates.Tests;

public class ValidateApproversClassicReleaseFunctionTests
{
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IValidateInputService> _validateInputServiceMock = new();
    private readonly Mock<IDurableOrchestrationClient> _durableOrchestrationClient = new();
    private readonly Mock<IAzdoRestClient> _azdoRestClient = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly ValidateApproversClassicReleaseFunction _sut;

    public ValidateApproversClassicReleaseFunctionTests() =>
        _sut = new ValidateApproversClassicReleaseFunction(_azdoRestClient.Object, _validateInputServiceMock.Object,
            _loggingServiceMock.Object);

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_ValidateInput_ProjectIdIsNull()
    {
        // Arrange
        var projectId = It.IsAny<string>();
        const string releaseId = "1234";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, organizationUri, true))
            .Returns(new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {nameof(projectId)} was not provided in the URL.")));

        // Act
        var actual = await _sut.RunAsync(request, null, releaseId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_ValidateInput_ReleaseIdIsNull()
    {
        // Arrange
        const string projectId = "1234";
        var releaseId = It.IsAny<string>();
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, organizationUri, true))
            .Returns(new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A {nameof(releaseId)} was not provided in the URL.")));

        // Act
        var actual = await _sut.RunAsync(request, projectId, releaseId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_ValidateInput_ReleaseIdNotNumber()
    {
        // Arrange
        const string projectId = "1234";
        const string releaseId = "1234d";
        const string organizationUri = "https://dev.azure.com/raboweb-test/";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", organizationUri);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, organizationUri, true))
            .Returns(new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"The runId: '{releaseId}' provided in the URL is invalid. It should only consist of numbers.")));

        // Act
        var actual = await _sut.RunAsync(request, projectId, releaseId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_ValidateInput_OrganizationUriCantBeParsed()
    {
        // Arrange
        const string projectId = "57272";
        const string releaseId = "12345";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", string.Empty);

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, It.IsAny<string>(), true))
            .Returns(new BadRequestObjectResult(Core.InputValidation.Model.ErrorMessages.CreateArgumentExceptionErrorMessage(
                $"A 'PlanUrl' was not provided in the request header. " +
                $"PlanUrls can be provided by adding following to your request header:\n" +
                $"PlanUrl: $(system.CollectionUri)")));

        // Act
        var actual = await _sut.RunAsync(request, projectId, releaseId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_GetRelease_Returns_BadRequestObjectResult()
    {
        // Arrange
        const string projectId = "57272";
        const string releaseId = "12345";
        const string organization = "raboweb-test";

        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", "raboweb-test");

        var release = It.IsAny<Release>();

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, organization, true))
            .Returns(new OkObjectResult(organization));

        _azdoRestClient.Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);

        // Act
        var actual = await _sut.RunAsync(request, projectId, releaseId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(BadRequestObjectResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_RunAsync_HappyFlow()
    {
        // Arrange
        const string projectId = "57272";
        const string runId = "12345";
        const string organization = "raboweb-test";
        var request = CreateDefaultRequestWithHeaders();
        var release = new Release { Id = 1234, Name = "testName", ReleaseDefinition = new ReleaseDefinition() };

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organization, true)).Returns(new OkObjectResult(organization));

        _azdoRestClient.Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>())).ReturnsAsync(release);

        // Act
        var actual = await _sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        actual.ShouldBeOfType(typeof(OkResult));
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_ShouldCallOrchestratorAsync()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        const string projectId = "57272";
        const string runId = "12345";
        const string organization = "raboweb-test";

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, runId, organization, true))
            .Returns(new OkObjectResult(organization));

        var release = new Release { Id = 1234, Name = "testName", ReleaseDefinition = new ReleaseDefinition() };
        _azdoRestClient.Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>())).ReturnsAsync(release);

        // Act
        await _sut.RunAsync(request, projectId, runId, null, _durableOrchestrationClient.Object);

        // Assert
        _durableOrchestrationClient
            .Verify(m => m.StartNewAsync(nameof(ValidateApproversOrchestrator), It.IsAny<ValidateApproversAzdoData>()), Times.Once);
    }

    [Fact]
    public async Task ValidateApproversClassicReleaseFunction_ShouldCallOrchestratorWithCorrectOrganizationAsync()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        var projectId = _fixture.Create<string>();
        var releaseId = _fixture.Create<string>();
        const string organization = "raboweb-test";
        var smokeTestOrganization = _fixture.Create<string>();

        _validateInputServiceMock.Setup(m => m.ValidateInput(projectId, releaseId, organization, true)).Returns(new OkObjectResult(organization));

        var release = new Release { Id = 1234, Name = "testName", ReleaseDefinition = new ReleaseDefinition() };
        _azdoRestClient.Setup(x => x.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>())).ReturnsAsync(release);

        // Act
        await _sut.RunAsync(request, projectId, releaseId, smokeTestOrganization, _durableOrchestrationClient.Object);

        // Assert
        _durableOrchestrationClient
            .Verify(m => m.StartNewAsync(nameof(ValidateApproversOrchestrator), It.Is<ValidateApproversAzdoData>(x =>
                x.SmoketestOrganization == smokeTestOrganization
                && x.Organization == organization
                && x.RunId == null
                && x.Release == release
                && x.ProjectId == projectId
                && x.StageId == null
            )), Times.Once);
    }

    [Fact]
    private async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var request = CreateDefaultRequestWithHeaders();
        var projectId = _fixture.Create<string>();
        var releaseId = _fixture.Create<string>();
        var organization = _fixture.Create<string>();

        // Act
        var actual = () =>
            _sut.RunAsync(request, projectId, releaseId, organization, _durableOrchestrationClient.Object);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.ValidateGatesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
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