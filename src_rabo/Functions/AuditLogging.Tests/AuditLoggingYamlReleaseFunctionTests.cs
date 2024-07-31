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
using PipelineProcessType = Rabobank.Compliancy.Infra.AzdoClient.Model.Constants.PipelineProcessType;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class AuditLoggingYamlReleaseFunctionTests
{
    private const LogDestinations _logName = LogDestinations.AuditDeploymentLog;
    private const string _deploymentSucceededStatus = "succeeded";
    private const string _pipelineId = "1";
    private const string _stageName = "Stage 1";
    private const string _sm9ChangeId = "C000123456";
    private const string _sm9ChangeHash = "00aa0a00";
    private const string _sm9ChangeTag = _sm9ChangeId + "[" + _sm9ChangeHash + "]";
    private readonly Mock<IYamlHelper> _yamlHelper = new();
    private readonly Mock<IMonitorDecoratorService> _monitorDecoratorService = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IAzdoRestClient> _azdoRestClient = new();
    private readonly Mock<IYamlReleaseApproverService> _yamlReleaseApproverService = new();
    private readonly Mock<IPullRequestApproverService> _pullRequestApproverService = new();
    private readonly Mock<IBuildPipelineService> _buildPipelineService = new();
    private readonly Mock<IRepositoryService> _repoService = new();
    private readonly Mock<IEnumerable<IBuildPipelineRule>> _buildPipelineRules = new();
    private readonly Mock<IPipelineRegistrationRepository> _pipelineRegistrationRepository = new();
    private readonly Mock<IYamlReleaseDeploymentEventParser> _yamlReleaseDeploymentEventParser = new();

    [Theory]
    [InlineData("canceled")]
    [InlineData("skipped")]
    [InlineData("notDeployed")]
    public async Task ShouldNotScanCanceledOrSkippedDeployments(string deploymentStatus)
    {
        // arrange
        var evt = _fixture.Build<YamlReleaseDeploymentEvent>()
            .With(x => x.DeploymentStatus, deploymentStatus)
            .Create();

        _yamlReleaseDeploymentEventParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(evt);
        var registrationRepository = new Mock<IPipelineRegistrationRepository>();

        // act
        var function = new AuditLoggingYamlReleaseFunction(_azdoRestClient.Object, _loggingServiceMock.Object, _yamlReleaseDeploymentEventParser.Object,
            registrationRepository.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object, _buildPipelineService.Object, _repoService.Object, _buildPipelineRules.Object, _monitorDecoratorService.Object);
        await function.RunAsync(It.IsAny<string>());

        // assert
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorYamlReleaseAsync(evt.Organization, evt.ProjectId, evt.RunId,
                evt.StageName), Times.Once());
        registrationRepository
            .Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ShouldNotScanDeploymentsOfUnregisteredPipelineOrStage()
    {
        // arrange
        var evt = _fixture.Build<YamlReleaseDeploymentEvent>()
            .With(x => x.DeploymentStatus, _deploymentSucceededStatus)
            .Create();
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>(0).ToList();

        _yamlReleaseDeploymentEventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        _pipelineRegistrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);
        var azdoClient = new Mock<IAzdoRestClient>();

        // act
        var function = new AuditLoggingYamlReleaseFunction(azdoClient.Object, _loggingServiceMock.Object,
            _yamlReleaseDeploymentEventParser.Object, _pipelineRegistrationRepository.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object, _buildPipelineService.Object, _repoService.Object, _buildPipelineRules.Object, _monitorDecoratorService.Object);
        await function.RunAsync(It.IsAny<string>());

        // assert
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorYamlReleaseAsync(evt.Organization, evt.ProjectId, evt.RunId,
                evt.StageName), Times.Once());
        _pipelineRegistrationRepository
            .Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        azdoClient
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()),
                Times.Never);
    }

    [Fact]
    public async Task ShouldScanProductionDeploymentAndUploadCorrectReport()
    {
        // arrange
        var project = _fixture.Create<Project>();
        var sm9ChangeUrl = new Uri("http://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r" +
                                   "&query=number%3D%22" + _sm9ChangeId + "%22&action=&title=Change%20Request%20Details&" +
                                   "queryHash=" + _sm9ChangeHash);
        var pullRequestApproval = _fixture.Create<bool>();
        var pipelineApproval = _fixture.Create<bool>();
        var repoUrl = _fixture.Create<Uri>();

        var evt = _fixture.Build<YamlReleaseDeploymentEvent>()
            .With(x => x.PipelineId, _pipelineId)
            .With(x => x.StageName, _stageName)
            .With(x => x.DeploymentStatus, _deploymentSucceededStatus)
            .Create();
        var pipelineRegistrations = _fixture.Build<PipelineRegistration>()
            .With(x => x.PipelineId, _pipelineId)
            .With(x => x.StageId, _stageName)
            .CreateMany(1)
            .ToList();
        var pipelineRun = _fixture.Build<Build>()
            .With(x => x.Tags, new[] { _sm9ChangeTag })
            .Create();
        var buildPipeline = _fixture.Create<BuildDefinition>();
        buildPipeline.Links.Web.Href = new Uri("http://build-url.nl/");
        buildPipeline.Process.Type = PipelineProcessType.YamlPipeline;

        var expected = new AuditLoggingReport
        {
            Organization = evt.Organization,
            ProjectName = project.Name,
            ProjectId = evt.ProjectId,
            PipelineName = evt.PipelineName,
            PipelineId = evt.PipelineId,
            StageName = evt.StageName,
            StageId = evt.StageId,
            RunName = evt.RunName,
            RunId = evt.RunId,
            RunUrl = evt.RunUrl,
            DeploymentStatus = evt.DeploymentStatus,
            CreatedDate = evt.CreatedDate,
            CompletedOn = pipelineRun.StartTime,
            PipelineApproval = pipelineApproval,
            PullRequestApproval = pullRequestApproval,
            ArtifactIntegrity = true,
            Sm9ChangeId = _sm9ChangeId,
            Sm9ChangeUrl = sm9ChangeUrl,
            IsSox = pipelineRegistrations.IsSox(),
            CiIdentifier = pipelineRegistrations.CiIdentifiers(),
            CiName = pipelineRegistrations.CiNames(),
            BuildUrls = new List<Uri> { buildPipeline.Links.Web.Href },
            RepoUrls = new List<Uri> { repoUrl },
            SonarRan = false,
            FortifyRan = false
        }.ToExpectedObject();

        _yamlReleaseDeploymentEventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);
        _pipelineRegistrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);
        
        _azdoRestClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(project);
        _azdoRestClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRun);
        _yamlReleaseApproverService
            .Setup(c => c.HasApprovalAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineApproval);
        _pullRequestApproverService
            .Setup(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(pullRequestApproval);
        
        _buildPipelineService
            .Setup(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), null))
            .ReturnsAsync(new[] { buildPipeline });
        _buildPipelineService
            .Setup(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<BuildDefinition>>()))
            .ReturnsAsync(_fixture.CreateMany<Repository>(1).ToList());
        var repoUrlService = new Mock<IRepositoryService>();
        repoUrlService
            .Setup(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()))
            .ReturnsAsync(repoUrl);

        var buildPipelineRule = new BuildPipelineHasSonarqubeTask(_azdoRestClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);
        var fortifyBuildPipelineRule = new BuildPipelineHasFortifyTask(_azdoRestClient.Object, Create.MockedMemoryCache()
            , _yamlHelper.Object);

        var buildPipelineRules = new List<IBuildPipelineRule> { buildPipelineRule, fortifyBuildPipelineRule };

        // act
        var function = new AuditLoggingYamlReleaseFunction(_azdoRestClient.Object,
            _loggingServiceMock.Object, _yamlReleaseDeploymentEventParser.Object, _pipelineRegistrationRepository.Object,
            _yamlReleaseApproverService.Object, _pullRequestApproverService.Object, _buildPipelineService.Object,
            repoUrlService.Object, buildPipelineRules, _monitorDecoratorService.Object);
        await function.RunAsync(It.IsAny<string>());

        // assert
        _yamlReleaseDeploymentEventParser
            .Verify(c => c.Parse(It.IsAny<string>()), Times.Once);
        _monitorDecoratorService
            .Verify(m => m.MonitorDecoratorYamlReleaseAsync(evt.Organization, evt.ProjectId, evt.RunId,
                evt.StageName), Times.Once());
        _pipelineRegistrationRepository
            .Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _azdoRestClient
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Project>>(), It.IsAny<string>()),
                Times.Once);
        _azdoRestClient
            .Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Build>>(), It.IsAny<string>()),
                Times.Once);
        _yamlReleaseApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<Project>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        _pullRequestApproverService
            .Verify(c => c.HasApprovalAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        _buildPipelineService
            .Verify(s => s.GetLinkedPipelinesAsync(It.IsAny<string>(), It.IsAny<BuildDefinition>(), null),
                Times.Once);
        _buildPipelineService
            .Verify(s => s.GetLinkedRepositoriesAsync(It.IsAny<string>(), It.IsAny<IList<BuildDefinition>>()),
                Times.Once);
        repoUrlService
            .Verify(s => s.GetUrlAsync(It.IsAny<string>(), It.IsAny<Project>(), It.IsAny<Repository>()),
                Times.Once);
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(_logName, It.Is<AuditLoggingReport>(i =>
                expected.Equals(i))), Times.Once);
    }

    [Fact]
    public async Task ShouldUploadExceptionReportToValidateInputServiceForFailuresAndThrowException()
    {
        // arrange
        var evt = _fixture.Build<YamlReleaseDeploymentEvent>()
            .With(x => x.DeploymentStatus, _deploymentSucceededStatus)
            .Create();

        _yamlReleaseDeploymentEventParser.Setup(p => p.Parse(It.IsAny<string>()))
            .Returns(evt);
        _pipelineRegistrationRepository
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>(),
                It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        // act
        var function = new AuditLoggingYamlReleaseFunction(_azdoRestClient.Object, _loggingServiceMock.Object,
            _yamlReleaseDeploymentEventParser.Object, _pipelineRegistrationRepository.Object, _yamlReleaseApproverService.Object, _pullRequestApproverService.Object, _buildPipelineService.Object, _repoService.Object, _buildPipelineRules.Object, _monitorDecoratorService.Object);

        // assert
        await Assert.ThrowsAsync<Exception>(() => function.RunAsync(It.IsAny<string>()));
        _loggingServiceMock.Verify(c => c.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }
}