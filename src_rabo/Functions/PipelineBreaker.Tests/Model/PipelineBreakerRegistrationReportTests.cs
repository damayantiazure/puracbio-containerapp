#nullable enable

using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Domain.Enums;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Model;
using Rabobank.Compliancy.Infra.StorageClient.Exceptions;
using Rabobank.Compliancy.Infra.StorageClient.Model;
using System.Collections.Generic;
using System.Linq;
using static Rabobank.Compliancy.Infra.AzdoClient.Model.Constants;

namespace Rabobank.Compliancy.Functions.PipelineBreaker.Tests.Model;

public class PipelineBreakerRegistrationReportTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void PipelineBreakerRegistrationReport_InvalidTypeBlockingDisabled_ResultWarned()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.InvalidYamlPipeline);
        var sut = new PipelineBreakerRegistrationReport();
        sut.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, sut.RegistrationStatus, false);

        // Act
        var actual = sut.Result;

        // Assert
        Assert.Equal(PipelineBreakerResult.Warned, actual);
    }

    [Fact]
    public void PipelineBreakerRegistrationReport_InvalidTypeBlockingEnabled_ResultBlocked()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.InvalidYamlPipeline);
        var sut = new PipelineBreakerRegistrationReport();
        sut.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, sut.RegistrationStatus, true);

        // Act
        var actual = sut.Result;

        // Assert
        Assert.Equal(PipelineBreakerResult.Blocked, actual);
    }

    [Fact]
    public void PipelineBreakerRegistrationReport_YamlPipelineRegistered_ResultPassed()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.YamlReleasePipeline)
        {
            Stages = new List<StageReport> { new() { Id = "prod" } }
        };

        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var sut = new PipelineBreakerRegistrationReport
        {
            RegistrationStatus = PipelineBreakerExtensions.GetRegistrationStatus(registrations, runInfo)
        };
        sut.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, sut.RegistrationStatus, true);

        // Act
        var actual = sut.Result;

        // Assert
        Assert.Equal(PipelineBreakerResult.Passed, actual);
    }

    [Fact]
    public void PipelineBreakerRegistrationReport_ClassicReleasePipelineNotRegistered_ResultBlocked()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.ClassicReleasePipeline);
        var sut = new PipelineBreakerRegistrationReport();
        sut.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, sut.RegistrationStatus, true);

        // Act
        var actual = sut.Result;

        // Assert
        Assert.Equal(PipelineBreakerResult.Blocked, actual);
    }

    [Fact]
    public void PipelineBreakerRegistrationReport_YamlPipelineRegistered_ButRegisteredStageNotPresentInPipeline_ThrowsError()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.YamlReleasePipeline)
        {
            Stages = new List<StageReport> { new() { Id = "Stage1" } }
        };

        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        // Act + Assert
        Assert.Throws<NoRegisteredStagesFoundException>(() => new PipelineBreakerRegistrationReport
        {
            RegistrationStatus = PipelineBreakerExtensions.GetRegistrationStatus(registrations, runInfo)
        });
    }

    [Fact]
    public void PipelineBreakerRegistrationReport_YamlPipelineRegisteredDifferentCasingStageId_ResultPassed()
    {
        // Arrange
        var runInfo = new PipelineRunInfo("", "", "", "", ItemTypes.YamlReleasePipeline)
        {
            Stages = new List<StageReport> { new() { Id = "Prod" } }
        };

        var registrations = _fixture.Build<PipelineRegistration>()
            .With(r => r.PartitionKey, PipelineRegistration.Prod)
            .With(r => r.StageId, "prod")
            .CreateMany(1)
            .ToList();

        var sut = new PipelineBreakerRegistrationReport
        {
            RegistrationStatus = PipelineBreakerExtensions.GetRegistrationStatus(registrations, runInfo)
        };
        sut.Result = PipelineBreakerExtensions.GetResult(runInfo.PipelineType, sut.RegistrationStatus, true);

        // Act
        var actual = sut.Result;

        // Assert
        Assert.Equal(PipelineBreakerResult.Passed, actual);
    }
}