#nullable enable

using Rabobank.Compliancy.Functions.AuditLogging.Helpers;
using System;
using System.Globalization;
using System.IO;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Helpers;

public class ClassicReleaseDeploymentEventParserTests
{
    private readonly IClassicReleaseDeploymentEventParser _eventParser;

    public ClassicReleaseDeploymentEventParserTests() =>
        _eventParser = new ClassicReleaseDeploymentEventParser();

    [Fact]
    public void WhenJsonIsEmptyStringThenShouldReturnNull()
    {
        var evt = _eventParser.Parse(string.Empty);

        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsNullThenShouldReturnNull()
    {
        var evt = _eventParser.Parse(null);

        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsValidThenShouldReturnValidObject()
    {
        var evt = _eventParser.Parse(GetExampleEvent("ClassicReleaseEvent.json"));

        Assert.NotNull(evt);
        Assert.Equal("raboweb-test", evt.Organization);
        Assert.Equal("SOx compliant demo", evt.ProjectName);
        Assert.Equal("53410703-e2e5-4238-9025-233bd7c811b3", evt.ProjectId);
        Assert.Equal("Stage 1", evt.StageName);
        Assert.Equal("398", evt.ReleaseId);
        Assert.Equal("https://dev.azure.com/raboweb-test/SOx%20compliant%20demo/_release?releaseId=398&_a=release-summary", evt.ReleaseUrl);
        Assert.Equal(DateTime.Parse("2021-06-01T08:05:35.0204487Z", CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal), evt.CreatedDate);
    }

    [Fact]
    public void WhenJsonIsTestThenShouldReturnNull()
    {
        var evt = _eventParser.Parse(GetExampleEvent("ClassicReleaseTestEvent.json"));

        Assert.Null(evt);
    }

    public static string GetExampleEvent(string fileName)
    {
        var path = Path.Combine("Assets", fileName);
        return File.ReadAllText(path);
    }
}