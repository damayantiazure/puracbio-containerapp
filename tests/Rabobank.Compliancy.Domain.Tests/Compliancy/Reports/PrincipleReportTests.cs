using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class PrincipleReportTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Ctor_WithCorrectArgs_ShouldPopulateInstance()
    {
        // Arrange
        var name = _fixture.Create<string>();
        var hasRulesToCheck = _fixture.Create<bool>();
        var isSox = _fixture.Create<bool>();
        var scanDate = _fixture.Create<DateTime>();

        // Act
        var actual = new PrincipleReport(name, scanDate)
        {
            HasRulesToCheck = hasRulesToCheck,
            IsSox = isSox
        };

        // Assert
        actual.Name.Should().Be(name);
        actual.HasRulesToCheck.Should().Be(hasRulesToCheck);
        actual.IsSox.Should().Be(isSox);
    }

    [Fact]
    public void IsCompliant_HasRulesToCheckIsFalse_ShouldBeTrue()
    {
        // Arrange
        const bool hasRulesToCheck = false;
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<PrincipleReport>(c =>
            c.FromFactory<string, bool>((name, isSOx) => new PrincipleReport(name, scanDate)
            {
                HasRulesToCheck = hasRulesToCheck,
                IsSox = isSOx
            }));

        var sut = _fixture.Create<PrincipleReport>();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void IsCompliant_WithCompliantRuleReportsNull_ShouldBeFalse()
    {
        // Arrange
        const bool hasRulesToCheck = true;
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<PrincipleReport>(c =>
            c.FromFactory<string, bool>((name, isSOx) => new PrincipleReport(name, scanDate)
            {
                HasRulesToCheck = hasRulesToCheck,
                IsSox = isSOx
            }));

        var sut = _fixture.Build<PrincipleReport>()
            .Without(f => f.RuleReports)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsCompliant_WithCompliantRuleReportsEmpty_ShouldBeFalse()
    {
        // Arrange
        const bool hasRulesToCheck = true;
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<PrincipleReport>(c =>
            c.FromFactory<string, bool>((name, isSOx) => new PrincipleReport(name, scanDate)
            {
                HasRulesToCheck = hasRulesToCheck, 
                IsSox = isSOx
            }));

        var sut = _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports, Array.Empty<RuleReport>())
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void IsCompliant_WithRuleReports_ShouldHaveProperValue(bool compliant)
    {
        // Arrange
        var itemReports = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, compliant)
            .Without(f => f.Deviation)
            .CreateMany(1);
        var ruleReports = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, itemReports)
            .CreateMany(1);

        const bool hasRulesToCheck = true;
        var scanDate = _fixture.Create<DateTime>();

        _fixture.Customize<PrincipleReport>(c =>
            c.FromFactory<string, bool>((name, isSOx) => new PrincipleReport(name, scanDate)
            {
                HasRulesToCheck = hasRulesToCheck,
                IsSox = isSOx
            }));

        var sut = _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports, ruleReports.ToList)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().Be(compliant);
    }

    [Fact]
    public void HasDeviation_WithRuleReportsNull_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<PrincipleReport>()
            .Without(f => f.RuleReports)
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void HasDeviation_WithRuleReportsEmpty_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports, Array.Empty<RuleReport>())
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().BeFalse();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HasDeviation_WithRuleReports_ShouldHaveProperValue(bool hasDeviation)
    {
        // Arrange
        var itemReports = _fixture.Build<ItemReport>()
            .With(f => f.Deviation, hasDeviation ? _fixture.Create<DeviationReport>() : null)
            .CreateMany(1);
        var ruleReports = _fixture.Build<RuleReport>()
            .With(f => f.ItemReports, itemReports)
            .CreateMany(1);
        var sut = _fixture.Build<PrincipleReport>()
            .With(f => f.RuleReports, ruleReports.ToList)
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().Be(hasDeviation);
    }
}