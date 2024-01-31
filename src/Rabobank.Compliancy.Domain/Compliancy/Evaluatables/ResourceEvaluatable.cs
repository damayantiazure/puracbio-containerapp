using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

internal class ResourceEvaluatable : IEvaluatable
{
    private readonly bool _pipelineUsesNonBuildArtifact;
    private readonly PipelineProcessType _pipelineProcessType;

    public ResourceEvaluatable(Pipeline pipeline)
    {
        _pipelineProcessType = pipeline.DefinitionType;
        _pipelineUsesNonBuildArtifact = pipeline?.DefaultRunContent?.UsesNonBuildArtifact ?? false;
    }

    public bool PipelineUsesNonBuildArtifact =>
        _pipelineUsesNonBuildArtifact;

    public PipelineProcessType PipelineProcessType =>
        _pipelineProcessType;
}