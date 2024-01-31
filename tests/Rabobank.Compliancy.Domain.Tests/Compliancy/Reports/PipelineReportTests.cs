using Rabobank.Compliancy.Domain.Compliancy.Reports;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Reports;

public class PipelineReportTests
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
        var isProduction = _fixture.Create<bool>();

        // Act
        var actual = new PipelineReport(id, name, type, link, isProduction);

        // Assert
        actual.Id.Should().Be(id);
        actual.Name.Should().Be(name);
        actual.Type.Should().Be(type);
        actual.Link.Should().Be(link);
        actual.IsProduction.Should().Be(isProduction);
    }
}