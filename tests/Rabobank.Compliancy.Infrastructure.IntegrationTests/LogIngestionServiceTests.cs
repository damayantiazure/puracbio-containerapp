using AutoFixture;
using AutoFixture.Kernel;
using AutoMapper;
using Azure.Identity;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.IntegrationTests.Helpers;
using Rabobank.Compliancy.Infrastructure.InternalServices;
using Rabobank.Compliancy.Infrastructure.Mapping;
using System.Collections;

namespace Rabobank.Compliancy.Infrastructure.IntegrationTests;

[Trait("category", "integration")]
public class LogIngestionServiceTests
{
    private readonly Fixture _fixture = new();
    private readonly LogAnalyticsFixture _logAnalyticsFixture = new();

    public LogIngestionServiceTests() =>
        _fixture.Customizations.Add(new UtcNowSequenceGenerator());

    [Theory]
    [InlineData(typeof(AuditLoggingReport), LogDestinations.AuditDeploymentLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.AuditLoggingErrorLog)]
    [InlineData(typeof(HookFailureReport), LogDestinations.AuditLoggingHookFailureLog)]
    [InlineData(typeof(PoisonMessageReport), LogDestinations.AuditPoisonMessagesLog)]
    [InlineData(typeof(AuditLoggingPullRequestReport), LogDestinations.AuditPullRequestApproversLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.ComplianceScannerOnlineErrorLog)]
    [InlineData(typeof(CiReport), LogDestinations.CompliancyCis)]
    [InlineData(typeof(ItemReport), LogDestinations.CompliancyItems)]
    [InlineData(typeof(CompliancyPipelineReport), LogDestinations.CompliancyPipelines)]
    [InlineData(typeof(PrincipleReport), LogDestinations.CompliancyPrinciples)]
    [InlineData(typeof(RuleReport), LogDestinations.CompliancyRules)]
    [InlineData(typeof(DecoratorErrorReport), LogDestinations.DecoratorErrorLog)]
    [InlineData(typeof(DeviationQueueDto), LogDestinations.DeviationsLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.ErrorHandlingLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.Sm9ChangesErrorLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.ValidateGatesErrorLog)]
    [InlineData(typeof(PipelineBreakerReport), LogDestinations.PipelineBreakerComplianceLog)]
    [InlineData(typeof(ExceptionReport), LogDestinations.PipelineBreakerErrorLog)]
    [InlineData(typeof(PipelineBreakerRegistrationReport), LogDestinations.PipelineBreakerLog)]
    public async Task WriteLogEntryAsync_ShouldNotThrowException(Type modelType, LogDestinations destination)
    {
        // Arrange
        var sut = CreateSut();
        var entry = _fixture.Create(modelType, new SpecimenContext(_fixture));

        // Act
        var actual = () => sut.WriteLogEntryAsync(entry, destination);

        // Assert
        await actual.Should().NotThrowAsync();
    }

    [Fact]
    public async Task WriteLogEntriesAsync_ShouldNotThrowException()
    {
        // Arrange
        var modelType = typeof(AuditLoggingReport);
        const LogDestinations destination = LogDestinations.AuditDeploymentLog;
        var sut = CreateSut();
        var context = new SpecimenContext(_fixture);

        var entries = (IEnumerable)context.Resolve(
            new FiniteSequenceRequest(new SeededRequest(modelType, null), 3));

        // Act
        var actual = () => sut.WriteLogEntriesAsync(entries, destination);

        // Assert
        await actual.Should().NotThrowAsync();
    }

    private LogIngestionService CreateSut()
    {
        var mapper = new Mapper(new MapperConfiguration(c => c.AddProfile(new LoggingMappingProfile())));

        return new LogIngestionService(new IngestionClientFactory(
            new DefaultAzureCredential(), _logAnalyticsFixture.Config.Ingestion), mapper);
    }

    private class UtcNowSequenceGenerator : ISpecimenBuilder
    {
        public object Create(object request, ISpecimenContext context) =>
            request is Type type && type == typeof(DateTimeOffset)
                ? DateTimeOffset.UtcNow
                : new NoSpecimen();
    }
}