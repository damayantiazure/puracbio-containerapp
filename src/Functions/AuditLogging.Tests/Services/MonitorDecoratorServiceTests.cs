#nullable enable

using Newtonsoft.Json;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.AuditLogging.Services;
using Rabobank.Compliancy.Functions.AuditLogging.Tests.Assets;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System.IO;
using static Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model.Constants;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Services;

public class MonitorDecoratorServiceTests
{
    private readonly Mock<IAzdoRestClient> _azdoClientMock = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private const string Organization = "organization";
    private const string ProjectId = "projectId";
    private const string RunId = "runId";
    private const string StageName = "Resources";

    [Fact]
    public async Task MonitorDecoratorYamlReleaseAsync_ShouldFilterRecordsForStage()
    {
        // Arrange
        

        var data = File.ReadAllText(Path.Combine("Assets", "YamlBuildTimeline.json"));
        var timeline = JsonConvert.DeserializeObject<Timeline>(data);

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Timeline>>(), It.IsAny<string>()))!
            .ReturnsAsync(timeline);

        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()))
            .ReturnsAsync(DummyResponse.Response("Host is not running."));

        // Act
        var function = new MonitorDecoratorService(_azdoClientMock.Object, _loggingServiceMock.Object);
        await function.MonitorDecoratorYamlReleaseAsync(Organization, ProjectId, RunId, StageName);

        // Assert
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogDestinations.DecoratorErrorLog, It.Is<DecoratorErrorReport>(i =>
                    i.Organization == Organization &&
                    i.ProjectId == ProjectId &&
                    i.RunId == RunId &&
                    i.ReleaseId == null &&
                    i.PipelineType == ItemTypes.YamlReleasePipeline &&
                    i.StageName == StageName &&
                    i.Message == DummyResponse.Response("Host is not running."))),
                Times.Once);
    }

    [Fact]
    public async Task MonitorDecoratorYamlReleaseAsync_ShouldStopIfDecoratorDidNotRun()
    {
        // Arrange
        var data = File.ReadAllText(Path.Combine("Assets", "YamlBuildTimelineWithoutDecorator.json"));
        var timeline = JsonConvert.DeserializeObject<Timeline>(data);

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Timeline>>(), It.IsAny<string>()))!
            .ReturnsAsync(timeline);

        // Act
        var function = new MonitorDecoratorService(_azdoClientMock.Object, _loggingServiceMock.Object);
        await function.MonitorDecoratorYamlReleaseAsync(Organization, ProjectId, RunId, StageName);

        // Assert
        _azdoClientMock
            .Verify(x => x.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(DecoratorResultMessages.Passed, 0)]
    [InlineData(DecoratorResultMessages.NotRegistered, 0)]
    [InlineData(DecoratorResultMessages.InvalidYaml, 0)]
    [InlineData(DecoratorResultMessages.AlreadyScanned, 0)]
    [InlineData(DecoratorResultMessages.WarningAlreadyScanned, 0)]
    [InlineData(DecoratorResultMessages.ExclusionList, 0)]
    [InlineData(DecoratorResultMessages.NotCompliant, 0)]
    [InlineData(DecoratorResultMessages.WarningNotCompliant, 0)]
    [InlineData(DecoratorResultMessages.NoProdStagesFound, 0)]
    [InlineData($"{DecoratorErrors.ErrorPrefix}An internal server error occurred while executing the compliance scan", 0)]
    [InlineData("'curl' is not recognized as an internal or external command", 1)]
    [InlineData("The service is unavailable.", 1)]
    [InlineData("Function host is not running.", 1)]
    [InlineData("2023-02-03T07:37:43.8469682Z Your pipeline is registered as a PROD pipeline and none of the PROD stages registered in the CMDB are present in the current version of your pipeline. \r\n2023-02-03T07:37:43.8489312Z For the Compliancy Rules to be able to be evaluated at least 1 of the registered stages needs to be present.", 0)]
    public async Task MonitorDecoratorYamlReleaseAsync_ShouldOnlyLogRelevantErrorMessages(string responseMessage, int callCount)
    {
        // Arrange

        var data = File.ReadAllText(Path.Combine("Assets", "YamlBuildTimeline.json"));
        var timeline = JsonConvert.DeserializeObject<Timeline>(data);

        _azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Timeline>>(), It.IsAny<string>()))!
            .ReturnsAsync(timeline);

        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<IAzdoRequest>(), It.IsAny<string>()))
            .ReturnsAsync(DummyResponse.Response(responseMessage));

        // Act
        var function = new MonitorDecoratorService(_azdoClientMock.Object, _loggingServiceMock.Object);
        await function.MonitorDecoratorYamlReleaseAsync(Organization, ProjectId, RunId, StageName);

        // Assert
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogDestinations.DecoratorErrorLog, It.Is<DecoratorErrorReport>(i =>
                    i.Organization == Organization &&
                    i.ProjectId == ProjectId &&
                    i.RunId == RunId &&
                    i.ReleaseId == null &&
                    i.PipelineType == ItemTypes.YamlReleasePipeline &&
                    i.StageName == StageName &&
                    i.Message == DummyResponse.Response(responseMessage))),
                Times.Exactly(callCount));
    }

    [Fact]
    public async Task MonitorDecoratorClassicReleaseAsync_ShouldFilterTasksForStage()
    {
        // Arrange
        var stageName = "Stage 1";

        var data = File.ReadAllText(Path.Combine("Assets", "ClassicRelease.json"));
        var release = JsonConvert.DeserializeObject<Release>(data);

        _azdoClientMock
            .Setup(c => c.GetAsStringAsync(It.IsAny<VsrmRequest<object>>(), It.IsAny<string>()))
            .ReturnsAsync(DummyResponse.Response("Host is not running."));

        // Act
        var function = new MonitorDecoratorService(_azdoClientMock.Object, _loggingServiceMock.Object);
        await function.MonitorDecoratorClassicReleaseAsync(Organization, ProjectId, release, stageName);

        // Assert
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogDestinations.DecoratorErrorLog, It.Is<DecoratorErrorReport>(i =>
                    i.Organization == Organization &&
                    i.ProjectId == ProjectId &&
                    i.RunId == null &&
                    i.ReleaseId == release.Id.ToString() &&
                    i.PipelineType == ItemTypes.ClassicReleasePipeline &&
                    i.StageName == stageName &&
                    i.Message == DummyResponse.Response("Host is not running."))),
                Times.Once);
    }

    [Fact]
    public async Task MonitorDecoratorClassicReleaseAsync_ShouldStopIfDecoratorDidNotRun()
    {
        // Arrange
        var stageName = "Stage 1";
        var release = _fixture.Create<Release>();

        // Act
        var function = new MonitorDecoratorService(_azdoClientMock.Object, _loggingServiceMock.Object);
        await function.MonitorDecoratorClassicReleaseAsync(Organization, ProjectId, release, stageName);

        // Assert
        _azdoClientMock
            .Verify(x => x.GetAsStringAsync(It.IsAny<VsrmRequest<object>>(), It.IsAny<string>()), Times.Never);
    }
}