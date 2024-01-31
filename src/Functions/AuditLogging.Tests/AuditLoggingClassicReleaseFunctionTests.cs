#nullable enable

using ExpectedObjects;
using MemoryCache.Testing.Moq;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Core.Approvals.Services;
using Rabobank.Compliancy.Core.PipelineResources.Helpers;
using Rabobank.Compliancy.Core.PipelineResources.Services;
using Rabobank.Compliancy.Core.Rules.Model;
using Rabobank.Compliancy.Core.Rules.Rules;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using Rabobank.Compliancy.Functions.AuditLogging.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Environment = Rabobank.Compliancy.Infra.AzdoClient.Response.Environment;
using PipelineProcessType = Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.PipelineProcessType;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class AuditLoggingClassicReleaseFunctionTests
{
    private const LogDestinations LogName = LogDestinations.AuditDeploymentLog;
    private const string PipelineId = "1";
    private const string StageId = "1";
    private const string StageName = "Stage 1";
    private const string DeploymentSucceededStatus = "succeeded";
    private const string Sm9ChangeId = "C000123456";
    private const string Sm9ChangeHash = "00aa0a00";
    private const string Sm9ChangeTag = Sm9ChangeId + "[" + Sm9ChangeHash + "]";

    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly Mock<IYamlHelper> _yamlHelper = new();
    private readonly Mock<IMonitorDecoratorService> _monitorDecoratorService = new();

    [Fact]
    public async Task ShouldNotScanEventsWhenParserReturnsNull()
    {
        // arrange
        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        var azdoClientMock = new Mock<IAzdoRestClient>();

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClientMock.Object, null, eventParser.Object,
            null, null, null, null, null, null, _monitorDecoratorService.Object);
        await function.RunAsync(null);

        // assert
        azdoClientMock.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("canceled")]
    [InlineData("skipped")]
    [InlineData("notDeployed")]
    public async Task ShouldNotScanCanceledOrSkippedDeployments(string deploymentStatus)
    {
        // arrange
        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .Create();
        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        var environments = _fixture.Build<Environment>()
            .With(e => e.Status, deploymentStatus)
            .With(e => e.Name, StageName)
            .CreateMany();

        var release = _fixture.Build<Release>()
            .With(r => r.Environments, environments)
            .Create();
        var azdoClientMock = new Mock<IAzdoRestClient>();
        azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);

        var registrationRepository = new Mock<IPipelineRegistrationRepository>();

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClientMock.Object, null, eventParser.Object,
            registrationRepository.Object, null, null, null, null, null, _monitorDecoratorService.Object);
        await function.RunAsync(null);

        // assert
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorClassicReleaseAsync(evt.Organization, evt.ProjectId,
                release, evt.StageName), Times.Once());
        registrationRepository
            .Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotScanDeploymentsOfUnregisteredPipelineOrStage()
    {
        // arrange
        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .Create();
        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        var release = _fixture.Build<Release>()
            .With(r => r.Environments, CreateEnvironments())
            .Create();
        var azdoClientMock = new Mock<IAzdoRestClient>();
        azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);

        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>(0).ToList();
        var registrationRepository = new Mock<IPipelineRegistrationRepository>();
        registrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var pipelineApproverService = new Mock<IClassicReleaseApproverService>();

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClientMock.Object, null,
            eventParser.Object, registrationRepository.Object, pipelineApproverService.Object, null, null, null, null,
            _monitorDecoratorService.Object);
        await function.RunAsync(null);

        // assert
        azdoClientMock
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()),
                Times.Once);
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorClassicReleaseAsync(evt.Organization, evt.ProjectId,
                release, evt.StageName), Times.Once());
        registrationRepository
            .Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        pipelineApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData("Build", "Build", true, true, true, true, 2)]
    [InlineData("Build", "Repo", false, true, true, true, 1)]
    [InlineData("Build", "Build", true, true, false, false, 2)]
    [InlineData("Build", "Build", true, false, false, false, 2)]
    public async Task ShouldScanProductionDeploymentAndUploadCorrectReport(
        string artifactType1, string artifactType2, bool expectedArtifactIntegrity,
        bool pullRequestApproval1, bool pullRequestApproval2, bool expectedPullRequestApproval, int callCount)
    {
        // arrange
        var sm9ChangeUrl = new Uri("http://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r" +
                                   "&query=number%3D%22" + Sm9ChangeId + "%22&action=&title=Change%20Request%20Details&" +
                                   "queryHash=" + Sm9ChangeHash);
        var pipelineApproval = _fixture.Create<bool>();
        var repoUrl = _fixture.Create<Uri>();

        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .Create();

        var environments = CreateEnvironments(StageId, 1);
        var releaseDefinition = _fixture.Build<ReleaseDefinition>()
            .With(d => d.Id, PipelineId)
            .Create();
        var artifacts = _fixture.Build<ArtifactReference>()
            .CreateMany(2)
            .ToList();
        artifacts[0].Type = artifactType1;
        artifacts[1].Type = artifactType2;

        var release = _fixture.Build<Release>()
            .With(r => r.Environments, environments)
            .With(r => r.ReleaseDefinition, releaseDefinition)
            .With(r => r.Artifacts, artifacts)
            .With(r => r.Tags, new[] { Sm9ChangeTag })
            .Create();

        var buildPipeline = _fixture.Create<BuildDefinition>();
        buildPipeline.Links.Web.Href = new Uri("http://build-url.nl/");
        buildPipeline.Process.Type = PipelineProcessType.GuiPipeline;

        var pipelineRegistrations = CreatePipelineRegistrations();

        var expected = new AuditLoggingReport
        {
            Organization = evt.Organization,
            ProjectName = evt.ProjectName,
            ProjectId = evt.ProjectId,
            PipelineName = release.ReleaseDefinition.Name,
            PipelineId = release.ReleaseDefinition.Id,
            StageName = evt.StageName,
            StageId = environments.First().DefinitionEnvironmentId,
            RunName = release.Name,
            RunId = evt.ReleaseId,
            RunUrl = evt.ReleaseUrl,
            DeploymentStatus = environments.First().Status,
            CreatedDate = evt.CreatedDate,
            CompletedOn = environments.First().CreatedOn,
            PipelineApproval = pipelineApproval,
            PullRequestApproval = expectedPullRequestApproval,
            ArtifactIntegrity = expectedArtifactIntegrity,
            Sm9ChangeId = Sm9ChangeId,
            Sm9ChangeUrl = sm9ChangeUrl,
            IsSox = pipelineRegistrations.IsSox(),
            CiIdentifier = pipelineRegistrations.CiIdentifiers(),
            CiName = pipelineRegistrations.CiNames(),
            BuildUrls = new List<Uri> { buildPipeline.Links.Web.Href },
            RepoUrls = new List<Uri> { repoUrl },
            SonarRan = false,
            FortifyRan = false
        }.ToExpectedObject();

        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);
        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);
        var registrationRepository = new Mock<IPipelineRegistrationRepository>();
        registrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);
        var pipelineApproverService = new Mock<IClassicReleaseApproverService>();
        pipelineApproverService
            .Setup(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineApproval);
        var pullRequestApproverService = new Mock<IPullRequestApproverService>();
        pullRequestApproverService
            .SetupSequence(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(pullRequestApproval1)
            .ReturnsAsync(pullRequestApproval2);
        var releasePipelineService = new Mock<IReleasePipelineService>();
        releasePipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), null))
            .ReturnsAsync(new[] { buildPipeline });
        releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(), It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(1).ToList());
        var repoUrlService = new Mock<IRepositoryService>();
        repoUrlService
            .Setup(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()))
            .ReturnsAsync(repoUrl);

        var buildPipelineRule = new BuildPipelineHasSonarqubeTask(azdoClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);
        var fortifyBuildPipelineRule = new BuildPipelineHasFortifyTask(azdoClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);

        var buildPipelineRules = new List<IBuildPipelineRule> { buildPipelineRule, fortifyBuildPipelineRule };

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClient.Object,
            _loggingServiceMock.Object, eventParser.Object, registrationRepository.Object,
            pipelineApproverService.Object, pullRequestApproverService.Object, releasePipelineService.Object,
            repoUrlService.Object, buildPipelineRules, _monitorDecoratorService.Object);
        await function.RunAsync(null);

        // assert
        eventParser
            .Verify(e => e.Parse(It.IsAny<string>()), Times.Once);
        azdoClient
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()),
                Times.Once);
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorClassicReleaseAsync(evt.Organization, evt.ProjectId,
                release, evt.StageName), Times.Once());
        registrationRepository
            .Verify(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        pipelineApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        pullRequestApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Exactly(callCount));
        releasePipelineService
            .Verify(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), null), Times.Once);
        releasePipelineService
            .Verify(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(), It.IsAny<IList<BuildDefinition>>()),
                Times.Once);
        repoUrlService
            .Verify(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()), Times.Once);
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogName, It.Is<AuditLoggingReport>(i => expected.Equals(i))), Times.Once);
    }

    [Fact]
    public async Task ShouldScanProductionDeploymentAndUploadCorrectReportForPipelinesWithoutResources()
    {
        // arrange
        var sm9ChangeUrl = new Uri("http://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r" +
                                   "&query=number%3D%22" + Sm9ChangeId + "%22&action=&title=Change%20Request%20Details&" +
                                   "queryHash=" + Sm9ChangeHash);
        var pipelineApproval = _fixture.Create<bool>();
        var buildUrls = _fixture.CreateMany<Uri>(0).ToList();
        var repoUrls = _fixture.CreateMany<Uri>(0).ToList();
        var sonarRan = false;
        var fortifyRan = false;

        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .Create();

        var environments = CreateEnvironments(StageId, 1);
        var releaseDefinition = _fixture.Build<ReleaseDefinition>()
            .With(d => d.Id, PipelineId)
            .Create();
        var artifacts = _fixture.Build<ArtifactReference>()
            .CreateMany(0);

        var release = _fixture.Build<Release>()
            .With(r => r.Environments, environments)
            .With(r => r.ReleaseDefinition, releaseDefinition)
            .With(r => r.Artifacts, artifacts)
            .With(r => r.Tags, new[] { Sm9ChangeTag })
            .Create();

        var pipelineRegistrations = CreatePipelineRegistrations();

        var expected = new AuditLoggingReport
        {
            Organization = evt.Organization,
            ProjectName = evt.ProjectName,
            ProjectId = evt.ProjectId,
            PipelineName = release.ReleaseDefinition.Name,
            PipelineId = release.ReleaseDefinition.Id,
            StageName = evt.StageName,
            StageId = environments.First().DefinitionEnvironmentId,
            RunName = release.Name,
            RunId = evt.ReleaseId,
            RunUrl = evt.ReleaseUrl,
            DeploymentStatus = environments.First().Status,
            CreatedDate = evt.CreatedDate,
            CompletedOn = environments.First().CreatedOn,
            PipelineApproval = pipelineApproval,
            PullRequestApproval = false,
            ArtifactIntegrity = false,
            Sm9ChangeId = Sm9ChangeId,
            Sm9ChangeUrl = sm9ChangeUrl,
            IsSox = pipelineRegistrations.IsSox(),
            CiIdentifier = pipelineRegistrations.CiIdentifiers(),
            CiName = pipelineRegistrations.CiNames(),
            BuildUrls = buildUrls,
            RepoUrls = repoUrls,
            SonarRan = sonarRan,
            FortifyRan = fortifyRan
        }.ToExpectedObject();

        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);
        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);
        var registrationRepository = new Mock<IPipelineRegistrationRepository>();
        registrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);
        var pipelineApproverService = new Mock<IClassicReleaseApproverService>();
        pipelineApproverService
            .Setup(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineApproval);
        var pullRequestApproverService = new Mock<IPullRequestApproverService>();
        var releasePipelineService = new Mock<IReleasePipelineService>();
        releasePipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());
        releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(), It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());
        var repoUrlService = new Mock<IRepositoryService>();

        var buildPipelineRule = new BuildPipelineHasSonarqubeTask(azdoClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);
        var fortifyBuildPipelineRule = new BuildPipelineHasFortifyTask(azdoClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);

        var buildPipelineRules = new List<IBuildPipelineRule> { buildPipelineRule, fortifyBuildPipelineRule };

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClient.Object,
            _loggingServiceMock.Object, eventParser.Object, registrationRepository.Object,
            pipelineApproverService.Object, pullRequestApproverService.Object, releasePipelineService.Object,
            repoUrlService.Object, buildPipelineRules, _monitorDecoratorService.Object);
        await function.RunAsync(null);

        // assert
        eventParser
            .Verify(e => e.Parse(It.IsAny<string>()), Times.Once);
        azdoClient
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()),
                Times.Once);
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorClassicReleaseAsync(evt.Organization, evt.ProjectId,
                release, evt.StageName), Times.Once());
        registrationRepository
            .Verify(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        pipelineApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        pullRequestApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Never);
        releasePipelineService
            .Verify(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), null), Times.Once);
        releasePipelineService
            .Verify(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(), It.IsAny<IList<BuildDefinition>>()),
                Times.Once);
        repoUrlService
            .Verify(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()), Times.Never);
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogName, It.Is<AuditLoggingReport>(i => expected.Equals(i))), Times.Once);
    }

    [Fact]
    public async Task ShouldUploadExceptionReportToLogAnalyticsForFailuresAndThrowException()
    {
        // arrange
        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .Create();
        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        var environment = _fixture.Build<Environment>()
            .With(e => e.Status, DeploymentSucceededStatus)
            .With(e => e.Name, StageName)
            .CreateMany();
        var release = _fixture.Build<Release>()
            .With(r => r.Environments, environment)
            .Create();
        var azdoClientMock = new Mock<IAzdoRestClient>();
        azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);

        var registrationRepository = new Mock<IPipelineRegistrationRepository>();
        registrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        // act
        var function = new AuditLoggingClassicReleaseFunction(azdoClientMock.Object, _loggingServiceMock.Object,
            eventParser.Object, registrationRepository.Object, null, null, null, null, null, _monitorDecoratorService.Object);

        // assert
        await Assert.ThrowsAsync<Exception>(() => function.RunAsync(null));
        _loggingServiceMock.Verify(c => c.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog
            , It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }

    [Fact]
    public async Task RunAsync_BuildPiplineFromDifferentProject_ShouldCallPullRequestApproverServiceWithCorrectProject()
    {
        // Arrange
        const string project2 = "project2";
        const string buildIdProject2 = "buildId";

        var pipelineRegistrations = CreatePipelineRegistrations();

        var registrationRepository = new Mock<IPipelineRegistrationRepository>();
        registrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var evt = _fixture.Build<ClassicReleaseDeploymentEvent>()
            .With(x => x.StageName, StageName)
            .With(x => x.ProjectId, "Project1")
            .Create();

        var eventParser = new Mock<IClassicReleaseDeploymentEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        var artifacts = _fixture.Build<ArtifactReference>()
            .With(e => e.Type, "build")
            .CreateMany(1);

        artifacts.First().DefinitionReference.Version.Id = buildIdProject2;
        artifacts.First().DefinitionReference.Project.Id = project2;

        var release = _fixture.Build<Release>()
            .With(r => r.Environments, CreateEnvironments())
            .With(r => r.Artifacts, artifacts)
            .Create();

        var azdoClientMock = new Mock<IAzdoRestClient>();
        azdoClientMock
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Release>>(), It.IsAny<string>()))
            .ReturnsAsync(release);

        var pipelineApproverService = new Mock<IClassicReleaseApproverService>();
        pipelineApproverService
            .Setup(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        var pullRequestApproverService = new Mock<IPullRequestApproverService>();
        var releasePipelineService = new Mock<IReleasePipelineService>();
        releasePipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<ReleaseDefinition>(), It.IsAny<string>(), null))
            .ReturnsAsync(_fixture.CreateMany<BuildDefinition>(0).ToList());
        releasePipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<ReleaseDefinition>>(), It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(0).ToList());

        var buildPipelineRule = new BuildPipelineHasSonarqubeTask(azdoClientMock.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);
        var fortifyBuildPipelineRule = new BuildPipelineHasFortifyTask(azdoClientMock.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);

        var buildPipelineRules = new List<IBuildPipelineRule> { buildPipelineRule, fortifyBuildPipelineRule };

        // Act
        var classicReleasefunction = new AuditLoggingClassicReleaseFunction(azdoClientMock.Object, _loggingServiceMock.Object,
            eventParser.Object, registrationRepository.Object, pipelineApproverService.Object, pullRequestApproverService.Object,
            releasePipelineService.Object, null, buildPipelineRules, _monitorDecoratorService.Object);

        await classicReleasefunction.RunAsync(null);

        // Assert
        pullRequestApproverService
            .Verify(v => v.HasApprovalAsync(project2, buildIdProject2, It.IsAny<string>()), Times.Once);
    }

    private IEnumerable<Environment> CreateEnvironments()
    {
        return CreateEnvironments("dummyStageId");
    }

    private IEnumerable<Environment> CreateEnvironments(string stageId, int numberOfEnvironments = 3)
    {
        return _fixture.Build<Environment>()
            .With(e => e.Status, DeploymentSucceededStatus)
            .With(e => e.Name, StageName)
            .With(e => e.DefinitionEnvironmentId, stageId)
            .CreateMany(numberOfEnvironments);
    }

    private List<PipelineRegistration> CreatePipelineRegistrations()
    {
        return _fixture.Build<PipelineRegistration>()
            .With(x => x.PipelineId, PipelineId)
            .With(x => x.StageId, StageId)
            .CreateMany(1)
            .ToList();
    }
}