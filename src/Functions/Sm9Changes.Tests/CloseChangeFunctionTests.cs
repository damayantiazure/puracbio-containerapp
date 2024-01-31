#nullable enable

using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Application;
using Rabobank.Compliancy.Functions.Sm9Changes.Exceptions;
using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using Rabobank.Compliancy.Functions.Sm9Changes.Services;
using Rabobank.Compliancy.Infra.Sm9Client.Change;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests;

public class CloseChangeFunctionTests
{
    private const string _validChangeId = "C012345678";
    private const int _validRunId = int.MaxValue;
    private readonly Mock<ICloseChangeProcess> _closeChangeProcessMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ISm9ChangesService> _sm9ChangeServiceMock = new();
    private readonly Sm9ChangesService _sm9ChangesService = new(Mock.Of<IChangeClient>());

    private readonly CloseChangeFunction _sut;
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly string _validOrganization;
    private readonly Guid _validProjectId = Guid.NewGuid();

    public CloseChangeFunctionTests()
    {
        _sut = new CloseChangeFunction(
            _loggingServiceMock.Object,
            _sm9ChangeServiceMock.Object,
            _closeChangeProcessMock.Object);
        _validOrganization = _fixture.Create<string>();
    }

    [Fact]
    public async Task IfInputValidationReturnsError_WriteToLogAnalytics()
    {
        // Arrange
        _sm9ChangeServiceMock
            .Setup(m => m.ValidateFunctionInput(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<int>()))
            .Throws<Exception>();

        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangeServiceMock.Object,
            _closeChangeProcessMock.Object);

        // Act
        var actual = () => sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _sm9ChangeServiceMock.Verify(
            m => m.ValidateFunctionInput(It.IsAny<HttpRequestMessage>(), It.IsAny<string>(), It.IsAny<Guid>(),
                It.IsAny<string>(), It.IsAny<int>()),
            Times.Once);
        _loggingServiceMock
            .Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task OrganizationNotProvided_ReturnsBadRequest()
    {
        // Arrange
        string? invalidOrganization = null;
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), invalidOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _loggingServiceMock
            .Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(),
                It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'organization' is not provided in the request url");
    }

    [Fact]
    public async Task ProjectIdInvalid_ReturnsBadRequest()
    {
        // Arrange
        var invalidProjectId = Guid.Empty;
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, invalidProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'projectId' is not provided in the request url");
    }

    [Fact]
    public async Task PipelineTypeNotProvided_ReturnsBadRequest()
    {
        // Arrange
        string? emptyPipelineType = null;
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            emptyPipelineType, _validRunId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'pipelineType' is not provided in the request url");
    }

    [Fact]
    public async Task PipelineTypeInvalid_ReturnsBadRequest()
    {
        // Arrange
        var invalidPipelineType = _fixture.Create<string>();
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            invalidPipelineType, _validRunId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()),
            Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("An invalid 'pipelineType' has been provided in the request url.");
    }

    [Fact]
    public async Task RunIdInvalid_ReturnsBadRequest()
    {
        // Arrange
        const int invalidRunId = int.MinValue;
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, invalidRunId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'runId' is not provided in the request url");
    }

    [Fact]
    public async Task RequestContentNotProvided_ReturnsBadRequest()
    {
        // Arrange
        var emptyRequest = new HttpRequestMessage();
        var sut = new CloseChangeFunction(_loggingServiceMock.Object, _sm9ChangesService,
            _closeChangeProcessMock.Object);

        // Act
        var actual = await sut.RunAsync(emptyRequest, _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _loggingServiceMock.Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(),
            It.IsAny<Exception>()), Times.Once);

        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("'Content' is not provided in the request message");
    }

    [Fact]
    public async Task GetUserInputAsync_CompletionCodeInvalid_ReturnsBadRequest()
    {
        // Arrange
        var faultyRequest = CreateHttpRequestWithContent(true, "0");

        // Act
        var actual = await _sut.RunAsync(faultyRequest, _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
        var resultValue = ((BadRequestObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain("The provided ClosureCode: '0' is invalid.");
    }

    [Fact]
    public async Task CloseChangeProcess_ThrowsChangeIdNotFoundException_ReturnsBadRequest()
    {
        // Arrange
        var faultyRequest = CreateHttpRequestWithContent(false);
        _closeChangeProcessMock
            .Setup(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .Throws<ChangeIdNotFoundException>();

        // Act
        var actual = await _sut.RunAsync(faultyRequest, _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task CloseChangeProcess_ThrowsChangePhaseValidationException_ReturnsBadRequest()
    {
        // Arrange
        var faultyRequest = CreateHttpRequestWithContent(false);
        _closeChangeProcessMock
            .Setup(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .Throws<ChangePhaseValidationException>();

        // Act
        var actual = await _sut.RunAsync(faultyRequest, _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        actual.ShouldBeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ValidRequest_IsClosedAndReturnsOkObjectResult()
    {
        // Arrange
        var validChangeIds = new[] { _validChangeId };
        var closedChanges = Array.Empty<string>();

        _closeChangeProcessMock
            .Setup(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .ReturnsAsync((validChangeIds, closedChanges));

        // Act
        var actual = await _sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _closeChangeProcessMock
            .Verify(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"The following SM9 changes have been closed: {_validChangeId}");
    }

    [Fact]
    public async Task ValidRequest_ChangesAlreadyClosed_ReturnsOkObjectResult()
    {
        // Arrange
        var validChangeIds = Array.Empty<string>();
        var closedChanges = new[] { _validChangeId };

        _closeChangeProcessMock
            .Setup(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .ReturnsAsync((validChangeIds, closedChanges));

        // Act
        var actual = await _sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _closeChangeProcessMock
            .Verify(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain(
            $"The following SM9 changes have been closed: .\r\nIgnored changeIds: {_validChangeId}.");
    }

    [Theory]
    [InlineData("C000000000")]
    [InlineData("C000000000,C111111111")]
    [InlineData("C000000000,C111111111,C222222222")]
    public async Task ClosingMultipleChanges_ReturnsOkObjectResult_WithAllChangeIds(string expectedOutput)
    {
        // Arrange
        var validChangeIds = expectedOutput.Split(",");
        var closedChanges = Array.Empty<string>();

        _closeChangeProcessMock
            .Setup(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .ReturnsAsync((validChangeIds, closedChanges));

        // Act
        var actual = await _sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        _closeChangeProcessMock
            .Verify(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()), Times.Once);

        actual.ShouldBeOfType<OkObjectResult>();
        var resultValue = ((OkObjectResult)actual).Value!.ToString();
        resultValue!.ShouldContain($"The following SM9 changes have been closed: {expectedOutput}");
    }

    [Fact]
    public async Task RunAsync_WithUnexpectedException_ShouldThrowException()
    {
        // Arrange
        var exceptionMessage = _fixture.Create<string>();
        _closeChangeProcessMock
            .Setup(mock => mock.CloseChangeAsync(It.IsAny<CloseChangeRequest>()))
            .Throws(new Exception(exceptionMessage));

        // Act
        var actual = () => _sut.RunAsync(CreateHttpRequestWithContent(), _validOrganization, _validProjectId,
            SM9Constants.BuildPipelineType, _validRunId);

        // Assert
        await actual.Should().ThrowAsync<Exception>();
        _closeChangeProcessMock
            .Verify(m => m.CloseChangeAsync(It.IsAny<CloseChangeRequest>()), Times.Once);
        _loggingServiceMock
            .Verify(m => m.LogExceptionAsync(LogDestinations.Sm9ChangesErrorLog,
                It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }

    private HttpRequestMessage CreateHttpRequestWithContent(bool validChangeId = true, string completionCode = "1") =>
        new()
        {
            Content = new StringContent(JsonConvert.SerializeObject(
                new CloseChangeDetails
                {
                    ChangeId = validChangeId
                        ? _validChangeId
                        : _fixture.Create<string>(),
                    CompletionCode = completionCode,
                    CompletionComments = _fixture.CreateMany<string>().ToArray()
                }))
        };
}