using Rabobank.Compliancy.Infra.AzdoClient.Extensions;
using Shouldly;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests;

public class YamlExtensionsTests
{
    [Fact]
    public void ToJson_YamlStringNull_ReturnsNonNullJObject()
    {
        // Arrange
        string yamlString = null;

        // Act
        var result = yamlString.ToJson();

        // Assert
        Assert.NotNull(result);
    }

    [Theory]
    [InlineData("YamlUnicodeChars.txt")]
    [InlineData("YamlNonPrintableChars.txt")]
    public void ToJson_YamlStringWithUnicode_OrNonprintableCharacters_ReturnsYamlStringEscaped(string filename)
    {
        // Arrange
        var path = System.IO.Path.Combine("Assets", filename);
        var yaml = System.IO.File.ReadAllText(path);

        // Act
        var result = yaml.ToJson();

        // Assert
        result.HasValues.ShouldBeTrue();
    }        

    [Fact]
    public void ToJson_YamlStringWithoutUnicode_ReturnsYamlString()
    {
        // Arrange
        var yamlString = "- task unittest";

        // Act
        var result = yamlString.ToJson();

        // Assert
        result.First.ToString().ShouldBe("task unittest");
    }

    [Fact]
    public void ToJson_InvalidYaml_ShouldReturnEmptyJToken()
    {
        // Arrange
        var yamlString ="{- task unittest}";

        // Act
        var result = yamlString.ToJson();

        // Assert
        Assert.NotNull(result);
    }
}