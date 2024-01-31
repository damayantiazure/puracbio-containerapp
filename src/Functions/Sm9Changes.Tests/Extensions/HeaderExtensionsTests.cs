#nullable enable

using Rabobank.Compliancy.Functions.Sm9Changes.Extensions;
using System.Net.Http;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Tests.Extensions;

public class HeaderExtensionsTests
{
    [Fact]
    public void BuildId_BuildIdPresent_ReturnsBuildId() 
    {
        // Arrange
        const string buildId = "1";
        var request = new HttpRequestMessage();
        request.Headers.Add("BuildId", buildId);

        // Act
        var result = request.Headers.BuildId();

        // Asset
        result.ShouldBe(buildId);
    }

    [Fact]
    public void BuildId_BuildIdNotPresent_ReturnsNull()
    {
        // Arrange
        var request = new HttpRequestMessage();
        request.Headers.Add("Unittest", "test");

        // Act
        var result = request.Headers.BuildId();

        // Asset
        result.ShouldBeNull();
    }

    [Fact]
    public void ReleaseId_ReleaseIdPresent_ReturnsReleaseId()
    {
        // Arrange
        const string releaseId = "1";
        var request = new HttpRequestMessage();
        request.Headers.Add("ReleaseId", releaseId);

        // Act
        var result = request.Headers.ReleaseId();

        // Asset
        result.ShouldBe(releaseId);
    }

    [Fact]
    public void ProjectId_ProjectIdPresent_ReturnsProjectId()
    {
        // Arrange
        const string projectId = "1";
        var request = new HttpRequestMessage();
        request.Headers.Add("ProjectId", projectId);

        // Act
        var result = request.Headers.ProjectId();

        // Asset
        result.ShouldBe(projectId);
    }

    [Fact]
    public void PlanUrl_PlanUrlPresent_ReturnsPlanUrl()
    {
        // Arrange
        const string planUrl = "http://unittest.nl";
        var request = new HttpRequestMessage();
        request.Headers.Add("PlanUrl", planUrl);
           
        // Act
        var result = request.Headers.PlanUrl();

        // Asset
        result.ShouldBe(planUrl);
    }
}