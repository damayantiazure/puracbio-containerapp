using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.Mapping;

namespace Rabobank.Compliancy.Infrastructure.Tests.Mapping;

public class DeviationReportLogRecordMappingProfileTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void Map_DeviationReportLogRecord_ShouldCorrectlyMapToDeviationLogEntry()
    {
        // Arrange            
        var deviationReportLogRecord = _fixture.Create<DeviationReportLogRecord>();
        var mapper = new Mapper(new MapperConfiguration(c =>
        {
            c.AddProfile<DeviationReportLogRecordMappingProfile>();
        }));

        // Act
        var actual = mapper.Map<DeviationQueueDto>(deviationReportLogRecord);

        // Assert            
        // BeEquivalentTo cannot be used because it cannot compare string with enum by default
        actual.CiIdentifier.Should().Be(deviationReportLogRecord.CiIdentifier);
        actual.RuleName.Should().Be(deviationReportLogRecord.RuleName);
        actual.Comment.Should().Be(deviationReportLogRecord.Comment);
        actual.ItemId.Should().Be(deviationReportLogRecord.ItemId);
        actual.ItemProjectId.Should().Be(deviationReportLogRecord.ItemProjectId);
        actual.ProjectId.Should().Be(deviationReportLogRecord.ProjectId);
        actual.Reason.Should().Be(deviationReportLogRecord.Reason.ToString());
        actual.ReasonNotApplicable.Should().Be(deviationReportLogRecord.ReasonNotApplicable.ToString());
        actual.ReasonNotApplicableOther.Should().Be(deviationReportLogRecord.ReasonNotApplicableOther!);
        actual.ReasonOther.Should().Be(deviationReportLogRecord.ReasonOther!);
        actual.RecordType.Should().Be(deviationReportLogRecord.RecordType.ToString());
    }
}
