#nullable enable

using AutoFixture;
using Moq;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Services;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Linq;
using Xunit;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;
using Request = Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Tests.Services;

public class ManageHooksServiceTests
{
    private const string _eventQueueStorageAccountName = "compliancy-account";
    private const string _otherAccountName = "other-account";
    private const string _yamlReleaseEventType = "ms.vss-pipelines.stage-state-changed-event";
    private const string _classicReleaseEventType = "ms.vss-release.deployment-completed-event";
    private const string _accountKey = "account-key";
    private const string _organization = "_organization";
    private readonly Mock<IAzdoRestClient> _azdoClient = new();

    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IPipelineRegistrationRepository> _pipelineRegistrationRepo = new();
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    [Theory]
    [InlineData(NotificationResult.failed, 0, 1, 0)]
    [InlineData(NotificationResult.failed, 1, 1, 1)]
    [InlineData(NotificationResult.failed, 3, 1, 3)]
    [InlineData(NotificationResult.failed, 3, 0, 0)]
    [InlineData(NotificationResult.filtered, 1, 1, 1)]
    [InlineData(NotificationResult.filtered, 3, 2, 0)]
    [InlineData(NotificationResult.pending, 1, 1, 0)]
    [InlineData(NotificationResult.succeeded, 1, 1, 0)]
    public async Task ManageHooksOrganizationAsync_ShouldLogAHookFailureReportForEveryHookThatFailedYesterday(
        NotificationResult hookResult, int hookCount, int daysAgo, int expectedReports)
    {
        // Arrange
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName));
        _fixture.Customize<Notification>(ctx => ctx
            .With(n => n.CreatedDate, DateTime.Now.AddDays(-daysAgo))
            .With(n => n.Details, _fixture.Build<NotificationDetails>()
                .With(n => n.PublisherInputs, _fixture.Build<PublisherInputs>()
                    .With(f => f.ProjectId, _fixture.Create<Guid>().ToString)
                    .Create())
                .Create())
            .With(n => n.Result, hookResult));

        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        var hooks = _fixture.CreateMany<Hook>(1);
        var notifications = _fixture.CreateMany<Notification>(hookCount);
        var notification = _fixture.Create<Notification>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Notification>>(), It.IsAny<string>()))
            .ReturnsAsync(notifications);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IAzdoRequest<Notification>>(), It.IsAny<string>()))
            .ReturnsAsync(notification);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(),
            It.IsAny<string>()), Times.Exactly(1));
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Notification>>(),
            It.IsAny<string>()), Times.Exactly(1));
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IAzdoRequest<Notification>>(),
            It.IsAny<string>()), Times.Exactly(expectedReports));
        _loggingServiceMock.Verify(c => c.LogInformationAsync(
                LogDestinations.AuditLoggingHookFailureLog, It.IsAny<HookFailureReport>()),
            Times.Exactly(expectedReports));
    }

    [Theory]
    [InlineData(_eventQueueStorageAccountName, "correct-project-id", 1)]
    [InlineData(_eventQueueStorageAccountName, "other-project-id", 0)]
    [InlineData(_otherAccountName, "correct-project-id", 0)]
    [InlineData(_otherAccountName, "other-project-id", 0)]
    public async Task ManageHooksOrganizationAsync_ShouldFilterHooksOnAccountNameAndProjectId(
        string accountName, string projectId, int timesClientCalled)
    {
        // Arrange
        var pipelineId = _fixture.Create<string>();
        _fixture.Customize<Project>(ctx => ctx
            .With(p => p.Id, "correct-project-id"));
        var projects = _fixture.CreateMany<Project>(1);
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.PipelineId, pipelineId)
            .Without(h => h.ReleaseDefinitionId));
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditYamlReleaseQueueName)
            .With(h => h.AccountName, accountName));
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        var hooks = _fixture.CreateMany<Hook>();
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>(0).ToList();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(projects);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _pipelineRegistrationRepo
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(),
            It.IsAny<string>()), Times.Once);
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(),
            It.IsAny<string>()), Times.Once);
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<ReleaseDefinition>>(),
            It.IsAny<string>()), Times.Exactly(timesClientCalled));
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(),
            It.IsAny<string>()), Times.Exactly(timesClientCalled));
    }

    [Theory]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(5)]
    public async Task ManageHooksOrganizationAsync_ShouldDeleteDuplicateHooks(int identicalHooks)
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        var pipelineId = _fixture.Create<string>();
        _fixture.Customize<Project>(ctx => ctx
            .With(p => p.Id, projectId));
        var projects = _fixture.CreateMany<Project>(1);
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.PipelineId, pipelineId)
            .Without(h => h.ReleaseDefinitionId));
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditYamlReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _yamlReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(identicalHooks);
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _fixture.Customize<PipelineRegistration>(composer => composer
            .With(r => r.PipelineId, pipelineId));
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        _fixture.Customize<BuildDefinition>(composer => composer
            .With(b => b.Id, pipelineId));
        var pipelines = _fixture.CreateMany<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(projects);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(pipelines);
        _pipelineRegistrationRepo
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(c => c.DeleteAsync(It.IsAny<IAzdoRequest<Hook>>(), It.IsAny<string>()),
            Times.Exactly(identicalHooks - 1));
    }

    [Theory]
    [InlineData("registered-pipeline", 0)]
    [InlineData("unregistered-pipeline", 1)]
    public async Task ManageHooksOrganizationAsync_ShouldDeleteInvalidHooks(string pipelineId, int timesClientCalled)
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        _fixture.Customize<Project>(ctx => ctx
            .With(p => p.Id, projectId));
        var projects = _fixture.CreateMany<Project>(1);
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.PipelineId, pipelineId)
            .Without(h => h.ReleaseDefinitionId));
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditYamlReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _yamlReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(1);
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _fixture.Customize<PipelineRegistration>(composer => composer
            .With(r => r.PipelineId, "registered-pipeline"));
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>().ToList();
        _fixture.Customize<BuildDefinition>(composer => composer
            .With(b => b.Id, pipelineId));
        var pipelines = _fixture.CreateMany<BuildDefinition>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(projects);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(pipelines);
        _pipelineRegistrationRepo
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(c => c.DeleteAsync(It.IsAny<IAzdoRequest<Hook>>(), It.IsAny<string>()),
            Times.Exactly(timesClientCalled));
    }

    [Theory]
    [InlineData("pipeline-id-with-hook", 0)]
    [InlineData("pipeline-id-without-hook", 1)]
    public async Task ManageHooksOrganizationAsync_ShouldCreateYamlHooksIfRegisteredPipelinesDoNotHaveHook(
        string pipelineId, int timesClientCalled)
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        var stageId = _fixture.Create<string>();
        _fixture.Customize<Project>(ctx => ctx
            .With(p => p.Id, projectId));
        var projects = _fixture.CreateMany<Project>(1);
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.PipelineId, "pipeline-id-with-hook")
            .Without(h => h.ReleaseDefinitionId));
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditYamlReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _yamlReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(1);
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName)
            .With(c => c.EventQueueStorageAccountKey, _accountKey));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _fixture.Customize<PipelineRegistration>(composer => composer
            .With(r => r.PipelineId, pipelineId)
            .With(r => r.StageId, stageId));
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>(1).ToList();
        _fixture.Customize<BuildDefinition>(composer => composer
            .With(b => b.Id, pipelineId));
        var pipelines = _fixture.CreateMany<BuildDefinition>(1);

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(projects);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<BuildDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(pipelines);
        _pipelineRegistrationRepo
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(x => x.PostAsync(It.IsAny<IAzdoRequest<Request.Hooks.Add.Body, Hook>>(),
                It.Is<Request.Hooks.Add.Body>(b =>
                    b.EventType == _yamlReleaseEventType &&
                    b.ConsumerInputs.QueueName == StorageQueueNames.AuditYamlReleaseQueueName &&
                    b.ConsumerInputs.AccountName == _eventQueueStorageAccountName &&
                    b.ConsumerInputs.AccountKey == _accountKey &&
                    b.ConsumerInputs.ResourceDetailsToSend == "all" &&
                    b.PublisherInputs.ProjectId == projectId), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Exactly(timesClientCalled));
    }

    [Theory]
    [InlineData("pipeline-id-with-hook", 0)]
    [InlineData("pipeline-id-without-hook", 1)]
    public async Task ManageHooksOrganizationAsync_ShouldCreateClassicHooksIfRegisteredPipelinesDoNotHaveHook(
        string pipelineId, int timesClientCalled)
    {
        // Arrange
        var projectId = _fixture.Create<string>();
        var stageId = _fixture.Create<int>().ToString();
        _fixture.Customize<Project>(ctx => ctx
            .With(p => p.Id, projectId));
        var projects = _fixture.CreateMany<Project>(1);
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .Without(h => h.PipelineId)
            .With(h => h.ReleaseDefinitionId, "pipeline-id-with-hook"));
        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditClassicReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _classicReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(1);
        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName)
            .With(c => c.EventQueueStorageAccountKey, _accountKey));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _fixture.Customize<PipelineRegistration>(composer => composer
            .With(r => r.PipelineId, pipelineId)
            .With(r => r.StageId, stageId));
        var pipelineRegistrations = _fixture.CreateMany<PipelineRegistration>(1).ToList();
        _fixture.Customize<ReleaseDefinition>(composer => composer
            .With(r => r.Id, pipelineId));
        var pipelines = _fixture.CreateMany<ReleaseDefinition>(1);

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ReturnsAsync(projects);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<ReleaseDefinition>>(), It.IsAny<string>()))
            .ReturnsAsync(pipelines);
        _pipelineRegistrationRepo
            .Setup(r => r.GetAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(pipelineRegistrations);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.ManageHooksOrganizationAsync(_organization);

        // Assert
        _azdoClient.Verify(x => x.PostAsync(It.IsAny<IAzdoRequest<Request.Hooks.Add.Body, Hook>>(),
                It.Is<Request.Hooks.Add.Body>(b =>
                    b.EventType == _classicReleaseEventType &&
                    b.ConsumerInputs.QueueName == StorageQueueNames.AuditClassicReleaseQueueName &&
                    b.ConsumerInputs.AccountName == _eventQueueStorageAccountName &&
                    b.ConsumerInputs.AccountKey == _accountKey &&
                    b.ConsumerInputs.ResourceDetailsToSend == "minimal" &&
                    b.PublisherInputs.ProjectId == projectId), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Exactly(timesClientCalled));
    }

    [Fact]
    public async Task ManageHooksOrganizationAsync_ShouldUploadExceptionReportToLogAnalyticsForFailuresAndThrowException()
    {
        // Arrange
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Project>>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() => sut.ManageHooksOrganizationAsync(_organization));

        // Assert
        _loggingServiceMock.Verify(c => c.LogExceptionAsync(
            LogDestinations.AuditLoggingErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }

    [Theory]
    [InlineData("pipeline-id-with-hook", 0)]
    [InlineData("pipeline-id-without-hook", 1)]
    public async Task CreateHookAsync_ShouldCreateClassicHookIfRegisteredPipelineDoNotHaveHook(
        string pipelineId, int timesClientCalled)
    {
        // Arrange
        var projectId = _fixture.Create<string>();

        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditClassicReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.ReleaseDefinitionId, "pipeline-id-with-hook"));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _classicReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(1);

        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName)
            .With(c => c.EventQueueStorageAccountKey, _accountKey));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.CreateHookAsync(_organization, projectId, ItemTypes.ClassicReleasePipeline, pipelineId);

        // Assert
        _azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(),
            It.IsAny<string>()), Times.Once);

        _azdoClient.Verify(x => x.PostAsync(It.IsAny<IAzdoRequest<Request.Hooks.Add.Body, Hook>>(),
                It.Is<Request.Hooks.Add.Body>(b =>
                    b.EventType == _classicReleaseEventType &&
                    b.ConsumerInputs.QueueName == StorageQueueNames.AuditClassicReleaseQueueName &&
                    b.ConsumerInputs.AccountName == _eventQueueStorageAccountName &&
                    b.ConsumerInputs.AccountKey == _accountKey &&
                    b.ConsumerInputs.ResourceDetailsToSend == "minimal" &&
                    b.PublisherInputs.ProjectId == projectId), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Exactly(timesClientCalled));
    }

    [Theory]
    [InlineData("pipeline-id-with-hook", 0)]
    [InlineData("pipeline-id-without-hook", 1)]
    public async Task CreateHookAsync_ShouldCreateYamlHookIfRegisteredPipelineDoNotHaveHook(
        string pipelineId, int timesClientCalled)
    {
        // Arrange
        var projectId = _fixture.Create<string>();

        _fixture.Customize<ConsumerInputs>(ctx => ctx
            .With(h => h.QueueName, StorageQueueNames.AuditYamlReleaseQueueName)
            .With(h => h.AccountName, _eventQueueStorageAccountName));
        _fixture.Customize<PublisherInputs>(ctx => ctx
            .With(h => h.ProjectId, projectId)
            .With(h => h.PipelineId, "pipeline-id-with-hook"));
        _fixture.Customize<Hook>(composer => composer
            .With(h => h.EventType, _yamlReleaseEventType));
        var hooks = _fixture.CreateMany<Hook>(1);

        _fixture.Customize<StorageClientConfig>(composer => composer
            .With(c => c.EventQueueStorageAccountName, _eventQueueStorageAccountName)
            .With(c => c.EventQueueStorageAccountKey, _accountKey));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await sut.CreateHookAsync(_organization, projectId, ItemTypes.YamlReleasePipeline, pipelineId);

        // Assert
        _azdoClient.Verify(x => x.PostAsync(It.IsAny<IAzdoRequest<Request.Hooks.Add.Body, Hook>>(),
                It.Is<Request.Hooks.Add.Body>(b =>
                    b.EventType == _yamlReleaseEventType &&
                    b.ConsumerInputs.QueueName == StorageQueueNames.AuditYamlReleaseQueueName &&
                    b.ConsumerInputs.AccountName == _eventQueueStorageAccountName &&
                    b.ConsumerInputs.AccountKey == _accountKey &&
                    b.ConsumerInputs.ResourceDetailsToSend == "all" &&
                    b.PublisherInputs.ProjectId == projectId), It.IsAny<string>(), It.IsAny<bool>()),
            Times.Exactly(timesClientCalled));
    }

    [Fact]
    public async Task CreateHookAsync_ShouldUploadExceptionReportToLogAnalyticsForFailuresAndThrowException()
    {
        // Arrange
        var pipelineId = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        const string pipelineType = ItemTypes.ClassicReleasePipeline;
        var storageClientConfig = _fixture.Create<StorageClientConfig>();

        _azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception());

        var sut = new ManageHooksService(_azdoClient.Object, storageClientConfig,
            _loggingServiceMock.Object, _pipelineRegistrationRepo.Object);

        // Act
        await Assert.ThrowsAsync<Exception>(() =>
            sut.CreateHookAsync(_organization, projectId, pipelineType, pipelineId));

        // Assert
        _loggingServiceMock.Verify(
            c => c.LogExceptionAsync(LogDestinations.AuditLoggingHookFailureLog,
                It.IsAny<Exception>(), It.IsAny<ExceptionBaseMetaInformation>(), pipelineId, pipelineType), Times.Once);
    }
}