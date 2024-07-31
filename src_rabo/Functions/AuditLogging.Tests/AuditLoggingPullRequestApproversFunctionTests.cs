#nullable enable

using ExpectedObjects;
using Rabobank.Compliancy.Application.Services;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Domain.Exceptions;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using Rabobank.Compliancy.Functions.AuditLogging.Model;
using System;
using System.Collections.Generic;
using Task = System.Threading.Tasks.Task;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests;

public class AuditLoggingPullRequestApproversFunctionTests
{
    private const LogDestinations LogName = LogDestinations.AuditPullRequestApproversLog;

    private readonly IFixture _fixture = new Fixture();
    private readonly Mock<IPullRequestMergedEventParser> _pullRequestMergedEventParserMock = new();
    private readonly AuditLoggingPullRequestApproversFunction _sut;
    private readonly Mock<ILoggingService> _loggingServiceMock = new();

    public AuditLoggingPullRequestApproversFunctionTests()
    {
        _sut = new AuditLoggingPullRequestApproversFunction(_loggingServiceMock.Object
            , _pullRequestMergedEventParserMock.Object);
    }

    [Fact]
    public async Task ShouldLogPullRequestApprovalData()
    {
        // Arrange
        var evt = _fixture.Build<PullRequestMergedEvent>()
            .With(x => x.Approvers, new List<string> { "approver@rabobank.nl" })
            .With(x => x.Status, "completed")
            .Create();

        var expected = new AuditLoggingPullRequestReport
        {
            Approvers = evt.Approvers,
            ClosedDate = evt.ClosedDate,
            CreationDate = evt.CreationDate,
            LastMergeCommitId = evt.LastMergeCommitId,
            LastMergeSourceCommit = evt.LastMergeSourceCommit,
            LastMergeTargetCommit = evt.LastMergeTargetCommit,
            Organization = evt.Organization,
            ProjectId = evt.ProjectId,
            ProjectName = evt.ProjectName,
            PullRequestId = evt.PullRequestId,
            PullRequestUrl = evt.PullRequestUrl,
            RepositoryId = evt.RepositoryId,
            RepositoryUrl = evt.RepositoryUrl,
            Status = evt.Status,
            CreatedBy = evt.CreatedBy,
            ClosedBy = evt.ClosedBy
        }.ToExpectedObject();

        _pullRequestMergedEventParserMock
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        // Act
        await _sut.RunAsync(null);

        // Assert
        _pullRequestMergedEventParserMock
            .Verify(e => e.Parse(It.IsAny<string>()), Times.Once);
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogName, It.Is<AuditLoggingPullRequestReport>(i =>
                expected.Equals(i))), Times.Once);
    }

    [Fact]
    public async Task ShouldDoNothingWhenPullRequestIsNotCompleted()
    {
        // Arrange
        var evt = _fixture.Build<PullRequestMergedEvent>()
            .With(x => x.Status, "active")
            .Create();

        _pullRequestMergedEventParserMock.Setup(e => e.Parse(It.IsAny<string>()))
            .Returns(evt);

        // Act
        await _sut.RunAsync(null);

        // Assert
        _pullRequestMergedEventParserMock
            .Verify(e => e.Parse(It.IsAny<string>()), Times.Once);
        _loggingServiceMock
            .Verify(x => x.LogInformationAsync(LogName, It.IsAny<AuditLoggingPullRequestReport>())
                , Times.Never);
    }

    [Fact]
    public async Task ShouldUploadExceptionReportToLogAnalyticsForFailuresAndThrowException()
    {
        // Arrange
        var eventParser = new Mock<IPullRequestMergedEventParser>();
        eventParser
            .Setup(e => e.Parse(It.IsAny<string>()))
            .Throws(new Exception());

        // Act & Assert
        await Assert.ThrowsAsync<NullReferenceException>(() => _sut.RunAsync(null));
        _loggingServiceMock.Verify(c => c.LogExceptionAsync(LogDestinations.AuditLoggingErrorLog,
            It.IsAny<ExceptionBaseMetaInformation>(), It.IsAny<Exception>()), Times.Once);
    }
}