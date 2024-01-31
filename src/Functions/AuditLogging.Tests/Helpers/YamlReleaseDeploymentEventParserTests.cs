#nullable enable

using System;
using System.Globalization;
using System.IO;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Helpers;

public class YamlReleaseDeploymentEventParserTests
{
    [Fact]
    public void WhenJsonIsEmptyStringThenShouldReturnNull()
    {
        var sut = new YamlReleaseDeploymentEventParser();
        var evt = sut.Parse(string.Empty);
        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsNullThenShouldReturnNull()
    {
        var sut = new YamlReleaseDeploymentEventParser();
        var evt = sut.Parse(null);
        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsValidThenShouldReturnValidObject()
    {
        var sut = new YamlReleaseDeploymentEventParser();
        var evt = sut.Parse(GetExampleEvent());

        Assert.NotNull(evt);
        Assert.Equal("raboweb-test", evt.Organization);
        Assert.Equal("53410703-e2e5-4238-9025-233bd7c811b3", evt.ProjectId);
        Assert.Equal("MultiStageExample CI", evt.PipelineName);
        Assert.Equal("312", evt.PipelineId);
        Assert.Equal("Production", evt.StageName);
        Assert.Equal("9fb31afb-34d7-5044-b937-b629e0824357", evt.StageId);
        Assert.Equal("20200421.4", evt.RunName);
        Assert.Equal("33102", evt.RunId);
        Assert.Equal("https://dev.azure.com/raboweb-test/53410703-e2e5-4238-9025-233bd7c811b3/_build/results?buildId=33102", 
            evt.RunUrl);
        Assert.Equal("succeeded", evt.DeploymentStatus);
        Assert.Equal(
            DateTime.Parse("2020-04-21T12:59:06.2929229Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal), evt.CreatedDate);
    }

    private static string GetExampleEvent()
    {
        var filePath = Path.Combine("Assets", "YamlReleaseEvent.json");
        return File.ReadAllText(filePath);
    }
}