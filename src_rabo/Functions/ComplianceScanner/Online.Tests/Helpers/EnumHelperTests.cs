using Rabobank.Compliancy.Domain.Extensions;
using Rabobank.Compliancy.Functions.ComplianceScanner.Online.Model;
using Shouldly;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Online.Tests.Helpers;

public class EnumHelperTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ParseEnumOrNull_WithValidValue_ShouldParseEnum()
    {
        // Arrange
        var fieldToUpdate = FieldToUpdate.CiIdentifier;

        // Act
        var actual = EnumHelper.ParseEnumOrNull<FieldToUpdate>(fieldToUpdate.ToString());

        // Assert
        actual.ShouldBe(fieldToUpdate);
    }

    [Fact]
    public void ParseEnumOrNull_WithValidToLowerCaseValue_ShouldParseEnum()
    {
        // Arrange
        var fieldToUpdate = FieldToUpdate.CiIdentifier;

        // Act
        var actual = EnumHelper.ParseEnumOrNull<FieldToUpdate>(fieldToUpdate.ToString().ToLower());

        // Assert
        actual.ShouldBe(fieldToUpdate);
    }

    [Fact]
    public void ParseEnumOrNull_WithValidToUpperCaseValue_ShouldParseEnum()
    {
        // Arrange
        var fieldToUpdate = FieldToUpdate.CiIdentifier;

        // Act
        var actual = EnumHelper.ParseEnumOrNull<FieldToUpdate>(fieldToUpdate.ToString().ToUpper());

        // Assert
        actual.ShouldBe(fieldToUpdate);
    }

    [Fact]
    public void ParseEnumOrNull_WithInValidValue_ShouldReturnNull()
    {
        // Arrange
        var value = _fixture.Create<string>();

        // Act
        var actual = EnumHelper.ParseEnumOrNull<FieldToUpdate>(value);

        // Assert
        actual.ShouldBeNull();
    }

    [Fact]
    public void ParseEnumOrDefault_WithInValidValue_ShouldReturnDefault()
    {
        // Arrange
        var value = _fixture.Create<string>();

        // Act
        var actual = EnumHelper.ParseEnumOrDefault<FieldToUpdate>(value);

        // Assert
        actual.ShouldBe(FieldToUpdate.CiIdentifier);
    }
}