using Rabobank.Compliancy.Domain.Compliancy;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;
using Rabobank.Compliancy.Domain.Compliancy.Rules;

namespace Rabobank.Compliancy.Domain.Tests.Compliancy.Rules;

public class ClassicReleasePipelineUsesBuildArtifactTests
{
    private static readonly ClassicReleasePipelineUsesBuildArtifact _sut = new ClassicReleasePipelineUsesBuildArtifact();

    [Fact]
    public void Evaluate_EmptyPipeline_ReturnsTrueResult()
    {
        // Arrange            
        var pipeline = new Pipeline();
        var resourceEvaluatable = new ResourceEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(resourceEvaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }

    [Fact]
    public void Evaluate_ClassicPipelineWithNonBuildArtifacts_ReturnsFalseResult()
    {
        // Arrange            
        var pipeline = new Pipeline()
        {
            DefinitionType = Enums.PipelineProcessType.DesignerRelease,
            DefaultRunContent = new PipelineBody { UsesNonBuildArtifact = true }
        };

        var resourceEvaluatable = new ResourceEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(resourceEvaluatable);

        // Assert
        result.Passed.Should().BeFalse();
    }

    [Fact]
    public void Evaluate_ClassicPipelineWithoutNonBuildArtifacts_ReturnsFalseResult()
    {
        // Arrange            
        var pipeline = new Pipeline()
        {
            DefinitionType = Enums.PipelineProcessType.DesignerRelease,
            DefaultRunContent = new PipelineBody { UsesNonBuildArtifact = false }
        };

        var resourceEvaluatable = new ResourceEvaluatable(pipeline);

        // Act
        var result = _sut.Evaluate(resourceEvaluatable);

        // Assert
        result.Passed.Should().BeTrue();
    }
}