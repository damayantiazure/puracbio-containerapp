#nullable enable

using System;
using System.Globalization;
using System.IO;
using Rabobank.Compliancy.Functions.AuditLogging.Helpers;

namespace Rabobank.Compliancy.Functions.AuditLogging.Tests.Helpers;

public class PullRequestMergedEventParserTests
{
    [Fact]
    public void WhenJsonIsEmptyStringThenShouldReturnNull()
    {
        var sut = new PullRequestMergedEventParser();
        var evt = sut.Parse(string.Empty);
        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsNullThenShouldReturnNull()
    {
        var sut = new PullRequestMergedEventParser();
        var evt = sut.Parse(null);
        Assert.Null(evt);
    }

    [Fact]
    public void WhenJsonIsValidThenShouldReturnValidObject()
    {
        var sut = new PullRequestMergedEventParser();
        var evt = sut.Parse(GetExampleEvent());

        Assert.NotNull(evt);
        Assert.Equal("raboweb-test", evt.Organization);
        Assert.Equal("53410703-e2e5-4238-9025-233bd7c811b3", evt.ProjectId);
        Assert.Equal("156", evt.PullRequestId);
        Assert.Equal("https://dev.azure.com/raboweb-test/53410703-e2e5-4238-9025-233bd7c811b3/_apis/git/repositories/97d7b870-a21b-461c-8085-208d84d1a1c1/pullRequests/156", evt.PullRequestUrl);
        Assert.Equal("97d7b870-a21b-461c-8085-208d84d1a1c1", evt.RepositoryId);
        Assert.Equal("https://dev.azure.com/raboweb-test/53410703-e2e5-4238-9025-233bd7c811b3/_apis/git/repositories/97d7b870-a21b-461c-8085-208d84d1a1c1", evt.RepositoryUrl);
        Assert.Equal("completed", evt.Status);
        Assert.Equal(new[] { "Chung.Lok.Lam@rabobank.nl", "Harmen.de.Rooij@rabobank.nl" }, evt.Approvers);
        Assert.Equal("71c08469bae86b87883dd3b9d6b45f4ed84603f3", evt.LastMergeCommitId);
        Assert.Equal("9b924b0b298df8e6924569eb64df1d09586e67e3", evt.LastMergeSourceCommit);
        Assert.Equal("02624f3186e170444eea30f4f7394d44cb25597c", evt.LastMergeTargetCommit);
        Assert.Equal(
            DateTime.Parse("2021-07-06T20:58:47.5904034Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal), evt.CreationDate);
        Assert.Equal(
            DateTime.Parse("2021-07-06T20:59:12.8716453Z", CultureInfo.InvariantCulture,
                DateTimeStyles.AdjustToUniversal), evt.ClosedDate);
        Assert.Equal("someone@rabobank.nl", evt.CreatedBy);
        Assert.Equal("Chung.Lok.Lam@rabobank.nl", evt.ClosedBy);
    }

    private static string GetExampleEvent()
    {
        var filePath = Path.Combine("Assets", "PullRequestMergedEvent.json");
        return File.ReadAllText(filePath);
    }
}