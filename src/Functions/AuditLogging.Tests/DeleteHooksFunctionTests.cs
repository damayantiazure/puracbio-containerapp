#nullable enable

using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infra.AzdoClient;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using Rabobank.Compliancy.Infra.StorageClient;
using System;
using System.Linq;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class DeleteHooksFunctionTests
{
    private const string EventQueueStorageAccountName = "compliancy-account";
    private const string OtherAccountName = "other-account";
    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<ILoggingService> _loggingService = new();

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task ShouldGetAndDeleteHooksForEveryOrganization(int organizationCount)
    {
        // Arrange
        var organizations = _fixture.CreateMany<string>(organizationCount).ToArray();
        _fixture.Customize<StorageClientConfig>(c => c
            .With(clientConfig => clientConfig.EventQueueStorageAccountName, EventQueueStorageAccountName));
        var storageClientConfig = _fixture.Create<StorageClientConfig>();
        _fixture.Customize<ConsumerInputs>(c => c
            .With(consumerInputs => consumerInputs.AccountName, EventQueueStorageAccountName));
        var hooks = _fixture.CreateMany<Hook>(1);

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), It.IsAny<string>()))
            .ReturnsAsync(hooks);

        // Act
        var function = new DeleteHooksFunction(organizations, azdoClient.Object,
            null, storageClientConfig);
        await function.RunAsync(null);

        // Assert
        azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(),
            It.IsAny<string>()), Times.Exactly(organizationCount));
        azdoClient.Verify(c => c.DeleteAsync(It.IsAny<IAzdoRequest<Hook>>(),
            It.IsAny<string>()), Times.Exactly(organizationCount));
    }

    [Theory]
    [InlineData(0, EventQueueStorageAccountName, 0)]
    [InlineData(1, EventQueueStorageAccountName, 1)]
    [InlineData(3, EventQueueStorageAccountName, 3)]
    [InlineData(3, OtherAccountName, 0)]
    public async Task ShouldDoDeleteCallForEveryAuditLoggingHookFoundButNotForOtherHooks(
        int hookCount, string accountName, int expected)
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var storageClientConfig = _fixture.Build<StorageClientConfig>()
            .With(c => c.EventQueueStorageAccountName, EventQueueStorageAccountName)
            .Create();
        _fixture.Customize<ConsumerInputs>(c => c
            .With(consumerInputs => consumerInputs.AccountName, accountName));
        var hooks = _fixture.CreateMany<Hook>(hookCount);

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), organization))
            .ReturnsAsync(hooks);

        // Act
        var function = new DeleteHooksFunction(new[] { organization }, azdoClient.Object,
            null, storageClientConfig);
        await function.RunAsync(null);

        // Assert
        azdoClient.Verify(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), organization),
            Times.Once);
        azdoClient.Verify(c => c.DeleteAsync(It.IsAny<IAzdoRequest<Hook>>(), organization),
            Times.Exactly(expected));
    }

    [Fact]
    public async Task ShouldUploadExceptionReportToLogAnalyticsForFailuresAndThrowException()
    {
        // Arrange
        var organization = _fixture.Create<string>();
        var storageClientConfig = _fixture.Create<StorageClientConfig>();

        var azdoClient = new Mock<IAzdoRestClient>();
        azdoClient
            .Setup(c => c.GetAsync(It.IsAny<IEnumerableRequest<Hook>>(), organization))
            .ThrowsAsync(new Exception());

        // Act
        var function = new DeleteHooksFunction(new[] { organization }, azdoClient.Object,
            _loggingService.Object, storageClientConfig);

        // Assert
        await Assert.ThrowsAsync<Exception>(() => function.RunAsync(null));
        _loggingService.Verify(c => c.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }
}