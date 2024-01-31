using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Infrastructure.Models;

namespace Rabobank.Compliancy.Infrastructure.Tests.Models;

public class DbbDeployTaskTests
{
    private const string DbbDeployProd = "dbb-deploy-prod";
    private readonly Guid _pipelineTaskId = new("206089fc-dcf1-4d0a-bc10-135adf95db3c");
    private readonly IFixture _fixture = new Fixture();

    [Fact]
    public void DbbDeployTask_PipelineTaskIsDbbDeployTask_ReturnsTrue()
    {
        // Arrange
        var task = new PipelineTask
        {
            Id = _pipelineTaskId,
            Name = "Dbb-Deploy-prod"
        };

        // Act
        var result = DbbDeployTask.IsDbbDeployTask(task);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void DbbDeployTask_PipelineTaskIsNotDbbDeployTask_ReturnsFalse()
    {
        // Arrange
        var task = _fixture.Create<PipelineTask>();

        // Act
        var result = DbbDeployTask.IsDbbDeployTask(task);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void DbbDeployTask_ReferencedProjectAndPipelineInInputs_ReturnsProjectIdAndPipelineId()
    {
        // Arrange
        var projectId = _fixture.Create<Guid>();
        const int pipelineId = 2;

        var task = new PipelineTask
        {
            Id = _pipelineTaskId,
            Name = DbbDeployProd,
            Inputs = new Dictionary<string, string> {
                { "projectId", projectId.ToString() },
                { "input2", "12" },
                { "pipelineId", pipelineId.ToString() }
            }
        };
        var dbbDeployTask = new DbbDeployTask(task);

        // Act
        var referencedProjectId = dbbDeployTask.ReferencedProject;
        var referencedPipelineId = dbbDeployTask.ReferencedPipelineId;

        // Assert
        referencedProjectId.Should().Be(projectId);
        referencedPipelineId.Should().Be(pipelineId);
    }

    [Fact]
    public void DbbDeployTask_NoInputsInPipelinTask_ReturnsProjectIdAndPipelineIdAsNull()
    {
        // Arrange
        var task = new PipelineTask
        {
            Id = _pipelineTaskId,
            Name = DbbDeployProd
        };

        var dbbDeployTask = new DbbDeployTask(task);

        // Act
        var referencedProjectId = dbbDeployTask.ReferencedProject;
        var referencedPipelineId = dbbDeployTask.ReferencedPipelineId;

        // Assert
        referencedProjectId.Should().BeNull();
        referencedPipelineId.Should().BeNull();
    }
}