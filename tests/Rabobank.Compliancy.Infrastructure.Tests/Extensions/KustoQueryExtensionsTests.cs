using Rabobank.Compliancy.Infrastructure.Extensions;

namespace Rabobank.Compliancy.Infrastructure.Tests.Extensions;

public class KustoQueryExtensionsTests
{
    [Theory]
    [InlineData(
        "log_table_CL",
        "union log_table*_CL")]
    [InlineData(
        "union log_table_CL",
        "union log_table*_CL")]
    [InlineData(
        "union log_table*_CL",
        "union log_table*_CL")]
    [InlineData(
        "log_table_CL | summarize count() by Type",
        "union log_table*_CL | summarize count() by Type")]
    [InlineData(
        "union log_table_CL | summarize count() by Type",
        "union log_table*_CL | summarize count() by Type")]
    [InlineData(
        "log_table_1_CL | union log_table_2_CL | summarize count() by Type",
        "union log_table_1*_CL | union log_table_2*_CL | summarize count() by Type")]
    public void ToWildCardUnion_ShouldCorrectlyTransformQuery(string kustoQuery, string expectedKustoQuery)
    {
        // Act
        var actual = kustoQuery.ToWildCardUnion();

        // Assert
        actual.Should().Be(expectedKustoQuery);
    }
}