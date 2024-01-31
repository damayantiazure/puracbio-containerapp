using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class CiReportTests
{
    private readonly Fixture _fixture = new();
    private readonly CiReport _sut;

    public CiReportTests() => 
        _sut = new CiReport(_fixture.Create<string>(), _fixture.Create<string>(), _fixture.Create<DateTime>());

    [Fact]
    public void IsCompliant_PrincipleReportsIsNull_ShouldBeFalse()
    {
        // Arrange
        _sut.PrincipleReports = null;

        // Act
        var actual = _sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsCompliant_PrincipleReportsIsEmpty_ShouldBeFalse()
    {
        // Arrange
        _sut.PrincipleReports = Array.Empty<PrincipleReport>();

        // Act
        var actual = _sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void IsCompliant_PrincipleReportsHasItems(bool isCompliant, bool expectedResult)
    {
        // Arrange
        _sut.PrincipleReports = _fixture
            .Build<PrincipleReport>()
            .FromFactory<string, DateTime>((name, scanData) => new PrincipleReport(name, scanData))
            .With(f => f.HasRulesToCheck, !isCompliant)
            .Without(f => f.RuleReports)
            .CreateMany(1);

        // Act
        var actual = _sut.IsCompliant;

        // Assert
        actual.Should().Be(expectedResult);
    }

    [Fact]
    public void IsSOxCompliant_PrincipleReportsIsNull_ShouldBeFalse()
    {
        // Arrange
        _sut.PrincipleReports = null;

        // Act
        var actual = _sut.IsSOxCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsSOxCompliant_PrincipleReportsIsEmpty_ShouldBeFalse()
    {
        // Arrange
        _sut.PrincipleReports = Array.Empty<PrincipleReport>();

        // Act
        var actual = _sut.IsSOxCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true, true)]
    [InlineData(true, false, false)]
    [InlineData(false, true, true)]
    [InlineData(false, false, true)]
    public void IsSOxCompliant_PrincipleReportsWithVariousOptions(bool isSox, bool isCompliant, bool expectedResult)
    {
        // Arrange
        _sut.PrincipleReports = _fixture
            .Build<PrincipleReport>()
            .FromFactory<string, DateTime>((name, scanData) => new PrincipleReport(name, scanData))
            .With(f => f.IsSox, isSox)
            .With(f => f.HasRulesToCheck, !isCompliant)
            .Without(f => f.RuleReports)
            .CreateMany(1);

        // Act
        var actual = _sut.IsSOxCompliant;

        // Assert
        actual.Should().Be(expectedResult);
    }

    [Fact]
    public void HasDeviation_PrincipleReportsIsNull_ShouldBeFalse()
    {
        // Arrange
        _sut.PrincipleReports = null;

        // Act
        var actual = _sut.HasDeviation;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true, true)]
    [InlineData(false, false)]
    public void HasDeviation_PrincipleReportsWithVariousOptions(bool hasDeviation, bool expectedResult)
    {
        // Arrange
        var itemReports = _fixture.Build<ItemReport>()
            .With(f => f.Deviation, hasDeviation ? _fixture.Create<DeviationReport>() : null)
            .CreateMany(1);
        var ruleReports = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, itemReports)
            .CreateMany(1);
        _sut.PrincipleReports = _fixture
            .Build<PrincipleReport>()
            .With(f => f.RuleReports, ruleReports.ToList)
            .CreateMany(1);

        // Act
        var actual = _sut.HasDeviation;

        // Assert
        actual.Should().Be(expectedResult);
    }
}