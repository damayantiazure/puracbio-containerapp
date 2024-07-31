#nullable enable

using Microsoft.AspNetCore.Mvc;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using Rabobank.Compliancy.Infra.Sm9Client.Change.Model;
using System;
using System.Collections.Generic;
using System.Net.Http;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests;

public class ValidateChangeFunctionTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IAzdoRestClient> _azdoRestClientMock = new();
    private readonly ValidateChangeFunction _sut;
    private const string _planUrl = "https://dev.azure.com/raboweb-test";
    private const string _changeId = "C012345678";

    public ValidateChangeFunctionTests()
    {
        _sut = new ValidateChangeFunction(_azdoRestClientMock.Object, _loggingServiceMock.Object, Mock.Of<ISm9ChangesService>());
    }

    [Fact]
    public async Task PlanUrlNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ReleaseId", _fixture.Create<int>().ToString());
        request.Headers.Add("ProjectId", _fixture.Create<string>());
        request.Headers.Add("BuildId", _fixture.Create<int>().ToString());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'PlanUrl' was not provided in the request headers.");
    }

    [Fact]
    public async Task PlanUrlInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ReleaseId", _fixture.Create<int>().ToString());
        request.Headers.Add("ProjectId", _fixture.Create<string>());
        var planUrl = _fixture.Create<string>();
        request.Headers.Add("PlanUrl", planUrl);
        request.Headers.Add("BuildId", _fixture.Create<int>().ToString());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"An invalid 'PlanUrl' was provided in the request headers: PlanUrl: {planUrl}.");
    }

    [Fact]
    public async Task ProjectIdNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ReleaseId", _fixture.Create<int>().ToString());
        request.Headers.Add("PlanUrl", _planUrl);
        request.Headers.Add("BuildId", _fixture.Create<int>().ToString());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'ProjectId' was not provided in the request headers.");
    }

    [Fact]
    public async Task BuildIdNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ReleaseId", _fixture.Create<int>().ToString());
        request.Headers.Add("PlanUrl", _planUrl);
        request.Headers.Add("ProjectId", _fixture.Create<string>());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'BuildId' was not provided in the request headers.");
    }

    [Fact]
    public async Task ReleaseIdNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ProjectId", _fixture.Create<string>());
        request.Headers.Add("PlanUrl", _planUrl);
        request.Headers.Add("BuildId", _fixture.Create<int>().ToString());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'ReleaseId' was not provided in the request headers.");
    }

    [Fact]
    public async Task BuildIdAndReleaseIdInvalid_ReturnsBadRequest()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("ProjectId", _fixture.Create<string>());
        request.Headers.Add("ReleaseId", _fixture.Create<string>());
        request.Headers.Add("PlanUrl", _planUrl);
        request.Headers.Add("BuildId", _fixture.Create<string>());

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("For either 'BuildId' or 'ReleaseId' an invalid value has been provided in the request headers.");
    }

    [Theory]
    [InlineData(SM9Constants.BuildPipelineType)]
    [InlineData(SM9Constants.ReleasePipelineType)]
    public async Task LowRiskChangeIdViaTags_ReturnsOkObjectResult(string pipelineType)
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(pipelineType);

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Tags { Value = new[] { SM9Constants.LowRiskChangeValue } });

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("The change is classified as a low-risk change.");
    }

    [Fact]
    public async Task Build_LowRiskChangeIdViaVariables_ReturnsOkObjectResult()
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(SM9Constants.BuildPipelineType);

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(new Build { Parameters = "{\"ChangeId\":\"low\"}" });

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("The change is classified as a low-risk change.");
    }

    [Fact]
    public async Task Release_LowRiskChangeIdViaVariables_ReturnsOkObjectResult()
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(SM9Constants.ReleasePipelineType);

        var variables = new Dictionary<string, VariableValue>
        {
            { "ChangeId", new VariableValue { Value = SM9Constants.LowRiskChangeValue } }
        };

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(new Release { Variables = variables });

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("The change is classified as a low-risk change.");
    }

    [Theory]
    [InlineData(SM9Constants.BuildPipelineType)]
    [InlineData(SM9Constants.ReleasePipelineType)]
    public async Task NoChangeId_ReturnsBadRequest(string pipelineType)
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(pipelineType);

        // Act
        var actual = await _sut.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Exactly(2));
        if (pipelineType == SM9Constants.BuildPipelineType)
        {
            _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Exactly(2));
        }
        else
        {
            _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Exactly(2));
        }

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("No valid ChangeId has been provided.");
    }

    [Fact]
    public async Task Build_InvalidChangePhase_ReturnsBadRequest()
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(SM9Constants.BuildPipelineType);

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(new Build { Parameters = $"{{\"ChangeId\":\"{_changeId}\"}}" });

        var changeClientMock = new Mock<IChangeClient>();
        changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        var sm9ChangesService = new Sm9ChangesService(changeClientMock.Object);

        var sut = new ValidateChangeFunction(_azdoRestClientMock.Object, _loggingServiceMock.Object, sm9ChangesService);

        // Act
        var actual = await sut.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Once);
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()), Times.Exactly(2));
        changeClientMock.Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"The following Changes do not have the correct Change Phase: {_changeId}");
    }

    [Fact]
    public async Task Release_ValidChangePhase_ReturnsOkObjectResult()
    {
        // Arrange
        var request = CreateHttpRequestWithHeaders(SM9Constants.ReleasePipelineType);

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Tags { Value = new[] { _changeId } });

        _fixture.Customize<ChangeInformation>(customizationComposer => customizationComposer
            .With(changeInformation => changeInformation.Phase, SM9Constants.DeploymentPhase));

        var changeClientMock = new Mock<IChangeClient>();
        changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ReturnsAsync(_fixture.Create<GetChangeByKeyResponse>());

        var sm9ChangesService = new Sm9ChangesService(changeClientMock.Object);

        var func = new ValidateChangeFunction(_azdoRestClientMock.Object, _loggingServiceMock.Object, sm9ChangesService);

        // Act
        var actual = await func.RunAsync(request);

        // Assert
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Exactly(2));
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Exactly(2));
        changeClientMock.Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"The verification is completed for changes: {_changeId}");
    }

    [Fact]
    public async Task UnexpectedException_ReturnsBadRequest()
    {
        // Arrange
        var exceptionMessage = _fixture.Create<string>();

        var request = CreateHttpRequestWithHeaders(SM9Constants.ReleasePipelineType);

        _azdoRestClientMock
            .Setup(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()))
            .ReturnsAsync(new Tags { Value = new[] { _changeId } });

        var changeClientMock = new Mock<IChangeClient>();
        changeClientMock
            .Setup(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()))
            .ThrowsAsync(new Exception(exceptionMessage));

        var sm9ChangesService = new Sm9ChangesService(changeClientMock.Object);

        var func = new ValidateChangeFunction(_azdoRestClientMock.Object, _loggingServiceMock.Object, sm9ChangesService);

        // Act
        var actual = () => func.RunAsync(request);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Tags>>(), It.IsAny<string>()), Times.Exactly(2));
        _azdoRestClientMock.Verify(m => m.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Exactly(2));
        changeClientMock.Verify(m => m.GetChangeByKeyAsync(It.IsAny<GetChangeByKeyRequestBody>()), Times.Once);
        _loggingServiceMock.Verify(x => x.LogExceptionAsync(
            LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionReport>()), Times.Once);
    }

    private HttpRequestMessage CreateHttpRequestWithHeaders(string pipelineType)
    {
        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", _planUrl);
        request.Headers.Add("ProjectId", _fixture.Create<string>());
        request.Headers.Add("BuildId", _fixture.Create<int>().ToString());
        request.Headers.Add("ReleaseId",
            pipelineType == SM9Constants.BuildPipelineType
                ? _fixture.Create<string>()
                : _fixture.Create<int>().ToString());

        return request;
    }
}