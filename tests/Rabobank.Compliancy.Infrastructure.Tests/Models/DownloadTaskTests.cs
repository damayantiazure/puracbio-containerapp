using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Models;

namespace Rabobank.Compliancy.Infrastructure.Tests.Models;

public class DownloadTaskTests
{
    private readonly IFixture _fixture = new Fixture();
    private static readonly Guid DownloadTaskId = new("61f2a582-95ae-4948-b34d-a1b3c4f6a737");

    [Fact]
    public void DownloadTask_PipelineTaskIsDownloadTask_ReturnsTrue()
    {
        // Arrange
        var task = new PipelineTask
        {
            Id = DownloadTaskId,
            Name = "DownloadPipelineArtifact"
        };

        // Act
        var result = DownloadPipelineArtifactTask.IsDownloadPipelineArtifactTask(task);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DownloadTask_PipelineTaskIsDownloadTask_ReturnsFalse()
    {
        // Arrange
        var task = _fixture.Create<PipelineTask>();

        // Act
        var result = DownloadPipelineArtifactTask.IsDownloadPipelineArtifactTask(task);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("pipeline")]
    [InlineData("definition")]
    public void DownloadTask_ReferencedProjectAndPipelineInInputs_ReturnsProjectIdAndPipelineId(string inputFieldNamePipeline)
    {
        // Arrange
        var projectId = _fixture.Create<Guid>();
        const int pipelineId = 2;

        var task = new PipelineTask
        {
            Id = DownloadTaskId,
            Name = "DownloadPipelineArtifact"
        };

        task.Inputs = new Dictionary<string, string> {
            { "Project", projectId.ToString() }, { "input2", "12" }, { inputFieldNamePipeline, pipelineId.ToString() }
        };
        var downloadTask = new DownloadPipelineArtifactTask(task);

        // Act
        var referencedProjectId = downloadTask.ReferencedProject;
        var referencedPipelineId = downloadTask.ReferencedPipelineId;

        // Assert
        referencedProjectId.Should().Be(projectId);
        referencedPipelineId.Should().Be(pipelineId);
    }
}