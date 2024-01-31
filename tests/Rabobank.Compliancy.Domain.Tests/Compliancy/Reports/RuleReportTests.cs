using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class RuleReportTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void IsCompliant_ItemReportsIsNull_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<RuleReport>()
            .Without(f => f.ItemReports)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsCompliant_ItemReportsIsEmpty_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, Array.Empty<ItemReport>())
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsCompliant_WithItemReports_ShouldHaveProperValue(bool compliant)
    {
        // Arrange
        var itemReports = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, compliant)
            .Without(f => f.Deviation)
            .CreateMany(1);

        var sut = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, itemReports)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().Be(compliant);
    }

    [Fact]
    public void HasDeviation_ItemReportsIsNull_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<RuleReport>()
            .Without(f => f.ItemReports)
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void HasDeviation_ItemReportsIsEmpty_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, Array.Empty<ItemReport>())
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HasDeviation_WithItemReports_ShouldHaveProperValue(bool hasDeviation)
    {
        // Arrange
        var itemReports = _fixture.Build<ItemReport>()
            .With(f => f.Deviation, hasDeviation ? _fixture.Create<DeviationReport>() : null)
            .Without(f => f.Deviation)
            .CreateMany(1);

        var sut = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, itemReports)
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().Be(hasDeviation);
    }
}