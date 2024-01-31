using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class ItemReportTests
{
    private readonly Fixture _fixture = new();

    [Fact]
    public void Ctor_WithCorrectArgs_ShouldPopulateInstance()
    {
        // Arrange
        var id = _fixture.Create<string>();
        var name = _fixture.Create<string>();
        var type = _fixture.Create<string>();
        var link = _fixture.Create<string>();
        var projectId = _fixture.Create<string>();
        var scanDate = _fixture.Create<DateTime>();

        // Act
        var actual = new ItemReport(id, name, projectId, scanDate)
        {
            Type = type,
            Link = link
        };

        // Assert
        actual.ItemId.Should().Be(id);
        actual.Name.Should().Be(name);
        actual.Type.Should().Be(type);
        actual.Link.Should().Be(link);
        actual.ProjectId.Should().Be(projectId);
    }

    [Fact]
    public void IsCompliant_NoHasDeviationAndIsCompliantForRuleFalse_ShouldBeFalse()
    {
        // Arrange
        var sut = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, false)
            .Without(f => f.Deviation)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeFalse();
    }

    [Fact]
    public void IsCompliant_NoHasDeviationAndIsCompliantForRuleTrue_ShouldBeTrue()
    {
        // Arrange
        var sut = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, true)
            .Without(f => f.Deviation)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void IsCompliant_HasDeviationAndIsCompliantForRuleFalse_ShouldBeTrue()
    {
        // Arrange
        var sut = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, false)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeTrue();
    }

    [Fact]
    public void IsCompliant_HasDeviationAndIsCompliantForRuleTrue_ShouldBeTrue()
    {
        // Arrange
        var sut = _fixture.Build<ItemReport>()
            .With(f => f.IsCompliantForRule, true)
            .Create();

        // Act
        var actual = sut.IsCompliant;

        // Assert
        actual.Should().BeTrue();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HasDeviation_ShouldReturnProperValue(bool hasDeviation)
    {
        // Arrange
        var sut = _fixture.Build<ItemReport>()
            .With(f => f.Deviation, hasDeviation ? _fixture.Create<DeviationReport>() : null)
            .Create();

        // Act
        var actual = sut.HasDeviation;

        // Assert
        actual.Should().Be(hasDeviation);
    }
}