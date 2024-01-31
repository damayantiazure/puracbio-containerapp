using AutoFixture.AutoMoq;
using AutoMapper;
using Rabobank.Compliancy.Domain.Compliancy.Deviations;
using Rabobank.Compliancy.Domain.Compliancy.Reports;
using Rabobank.Compliancy.Infrastructure.Dto.CompliancyReport;
using Rabobank.Compliancy.Infrastructure.Dto.Queue;
using Rabobank.Compliancy.Infrastructure.Mapping;

namespace Rabobank.Compliancy.Infrastructure.Tests.Mapping;

public class DeviationMappingProfileTests
{
    private readonly IFixture _fixture = new Fixture().Customize(new AutoMoqCustomization());

    [Fact]
    public void ShouldCorrectlyMapDeviationToDeviationQueueDto()
    {
        var deviation = _fixture.Create<Deviation>();
        var mapper = new Mapper(new MapperConfiguration(c => { c.AddProfile<DeviationMappingProfile>(); }));

        // Act
        var actual = mapper.Map<DeviationQueueDto>(deviation);

        // Assert            
        actual.Should().BeEquivalentTo(deviation, options => options
            .ExcludingMissingMembers()
            .Excluding(o => o.Reason)
            .Excluding(o => o.ReasonNotApplicable));

        actual.Reason.Should().Be(deviation.Reason.ToString());
        actual.ReasonNotApplicable.Should().Be(deviation.ReasonNotApplicable.ToString());
    }

    [Fact]
    public void ShouldCorrectlyMapDeviationToReportDto()
    {
        // Arrange
        var deviation = _fixture.Create<Deviation>();

        var sut = CreateMapper();

        // Act
        var actual = sut.Map<DeviationReportDto>(deviation);

        // Assert
        actual.Should().BeEquivalentTo(deviation, options => options
            .ExcludingMissingMembers()
            .Excluding(o => o.Reason)
            .Excluding(o => o.ReasonNotApplicable));

        actual.Reason.Should().Be(deviation.Reason.ToString());
        actual.ReasonNotApplicable.Should().Be(deviation.ReasonNotApplicable.ToString());
    }

    [Fact]
    public void ShouldCorrectlyMapDeviationToReport()
    {
        // Arrange
        var deviation = _fixture.Create<Deviation>();

        var sut = CreateMapper();

        // Act
        var actual = sut.Map<DeviationReport>(deviation);

        // Assert
        actual.Should().BeEquivalentTo(deviation, options => options
            .ExcludingMissingMembers()
            .Excluding(o => o.Reason)
            .Excluding(o => o.ReasonNotApplicable));

        actual.Reason.Should().Be(deviation.Reason.ToString());
        actual.ReasonNotApplicable.Should().Be(deviation.ReasonNotApplicable.ToString());
    }

    private static IMapper CreateMapper() =>
        new Mapper(new MapperConfiguration(cfg => { cfg.AddProfile<DeviationMappingProfile>(); }));
}