using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.Sm9Client.Tests.Cmdb.Model;

public class SupplementaryInformationTests
{
    private readonly string _jsonStringWithoutProfile = "{\"organization\":\"dummyorganization\",\"project\":\"dummyprojectid\",\"pipeline\":\"dummypipelineid\",\"stage\":\"dummystageid\"}";
    private readonly string _jsonStringWithProfile = "{\"organization\":\"dummyorganization\",\"project\":\"dummyprojectid\",\"pipeline\":\"dummypipelineid\",\"stage\":\"dummystageid\",\"profile\":\"dummyProfile\"}";
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ParseSupplementaryInfo_WithValidJson_WithoutProfile_ReturnsValidInstanceWithProfileNull()
    {
        // Act
        var result = SupplementaryInformation.ParseSupplementaryInfo(_jsonStringWithoutProfile);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Organization);
        Assert.NotEmpty(result.Organization);
        Assert.NotNull(result.Project);
        Assert.NotEmpty(result.Project);
        Assert.NotNull(result.Pipeline);
        Assert.NotEmpty(result.Pipeline);
        Assert.NotNull(result.Stage);
        Assert.NotEmpty(result.Stage);
        Assert.Null(result.Profile);
    }

    [Fact]
    public void ParseSupplementaryInfo_WithValidJson_WithProfile_ReturnsValidInstanceWithProfileNull()
    {
        // Act
        var result = SupplementaryInformation.ParseSupplementaryInfo(_jsonStringWithProfile);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Organization);
        Assert.NotNull(result.Project);
        Assert.NotNull(result.Pipeline);
        Assert.NotNull(result.Stage);
        Assert.NotNull(result.Profile);
        Assert.NotEmpty(result.Organization);
        Assert.NotEmpty(result.Project);
        Assert.NotEmpty(result.Pipeline);
        Assert.NotEmpty(result.Stage);
        Assert.NotEmpty(result.Profile);
    }

    [Fact]
    public void ToString_ReturnsOriginalJsonString_FromParseSupplementaryInformation()
    {
        // Arrange
        var jsonStrings = new[] { _jsonStringWithoutProfile, _jsonStringWithProfile };

        foreach (var jsonString in jsonStrings)
        {
            // Act
            var result = SupplementaryInformation.ParseSupplementaryInfo(jsonString);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(jsonString, result.ToString());
        }
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("{\"organization\":\"dummyorganization\",")]
    public void ParseSupplementaryInfo_WithInvalidJsonString_ReturnsNullInstance(string jsonString)
    {
        // Act
        var result = SupplementaryInformation.ParseSupplementaryInfo(jsonString);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData("{\"project\":\"dummyprojectid\",\"pipeline\":\"dummypipelineid\",\"stage\":\"dummystageid\",\"profile\":\"dummyProfile\"}")]
    [InlineData("{\"organization\":\"dummyorganization\",\"pipeline\":\"dummypipelineid\",\"stage\":\"dummystageid\",\"profile\":\"dummyProfile\"}")]
    [InlineData("{\"organization\":\"dummyorganization\",\"project\":\"dummyprojectid\",\"stage\":\"dummystageid\",\"profile\":\"dummyProfile\"}")]
    [InlineData("{\"organization\":\"dummyorganization\",\"project\":\"dummyprojectid\",\"pipeline\":\"dummypipelineid\",\"profile\":\"dummyProfile\"}")]
    public void ParseSupplementaryInformation_WithMissingRequiredProperty_ReturnsNullInstance(string jsonString)
    {
        // Act
        var result = SupplementaryInformation.ParseSupplementaryInfo(jsonString);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void ParseSupplementaryInfo_WhenInputValueIsNullOrEmptyWhitespaces_ShoulReturnNull()
    {
        // Act
        var actual = SupplementaryInformation.ParseSupplementaryInfo(string.Empty);

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public void ParseSupplementaryInfo_WhenJsonIsInvalid_ShouldReturnNull()
    {
        // Arrange
        var json = _fixture.Create<string>();

        // Act
        var actual = SupplementaryInformation.ParseSupplementaryInfo(json);

        // Assert
        actual.Should().BeNull();
    }

    [Fact]
    public void ParseSupplementaryInfo_WithNullOrWhiteSpaceParameters_ShouldReturnNull()
    {
        // Arrange
        var strWhiteSpace = " ";

        // Act
        var actual = SupplementaryInformation.ParseSupplementaryInfo(strWhiteSpace);

        // Assert
        actual.Should().BeNull();
    }
}