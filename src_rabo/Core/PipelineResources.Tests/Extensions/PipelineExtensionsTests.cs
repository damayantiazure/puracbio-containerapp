using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Core.PipelineResources.Extensions;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Core.PipelineResources.Tests.Extensions;

public class PipelineExtensionsTests
{
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void ToInputsDictionary_WithNoJTokenInstance_ShouldReturnEmptyCollection()
    {
        // Arrange
        JObject jtoken = null;

        // Act
        var actual = jtoken.ToInputsDictionary();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToInputsDictionary_WithJArrayInstance_ShouldReturnEmptyCollection()
    {
        // Arrange
        var jtoken = _fixture.Create<JArray>();

        // Act
        var actual = jtoken.ToInputsDictionary();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToInputsDictionary_WithJValueInstance_ShouldReturnEmptyCollection()
    {
        // Arrange
        var value = _fixture.Create<string>();
        var jtoken = new JValue(value);

        // Act
        var actual = jtoken.ToInputsDictionary();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToInputsDictionary_WithJPropertyInstance_ShouldReturnEmptyCollection()
    {
        // Arrange
        var value = _fixture.Create<string>();
        var jtoken = new JProperty(value);

        // Act
        var actual = jtoken.ToInputsDictionary();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToInputsDictionary_WithEmptyJTokenInstance_ShouldReturnEmptyCollection()
    {
        // Arrange
        var jtoken = _fixture.Create<JObject>();

        // Act
        var actual = jtoken.ToInputsDictionary();

        // Assert
        actual.Should().BeEmpty();
    }

    [Fact]
    public void ToInputsDictionary_WithValidJson_ShouldReturnFilledCollection()
    {
        // Arrange
        var json = @"
            {
              ""serviceEndpoint"": ""DummyConnection"",
              ""ucdVersion"": ""1"",
              ""organizationName"": ""DummyOrganization"",
              ""projectId"": ""DummyProjectId"",
              ""pipelineId"": ""DummyPipelineId""
            }";

        // Act
        var actual = JObject.Parse(json).ToInputsDictionary();

        // Assert
        actual.Should().NotBeEmpty();
        actual.Should().BeEquivalentTo(new Dictionary<string, string>
        {
            { "serviceEndpoint", "DummyConnection" },
            { "ucdVersion", "1"},
            { "organizationName", "DummyOrganization"},
            { "projectId", "DummyProjectId"},
            { "pipelineId", "DummyPipelineId"}
        });
    }
}