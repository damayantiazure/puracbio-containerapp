namespace Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

/// <inheritdoc/>
public class TaskContainingEvaluatable : IEvaluatable
{
    private readonly PipelineBody _body = null!;

    public TaskContainingEvaluatable(Pipeline pipeline)
    {
        _body = pipeline?.DefaultRunContent;
    }

    public TaskContainingEvaluatable(Run run)
    {
        _body = run?.RunBody;
    }

    public IEnumerable<PipelineTask> GetPipelineTasks()
    {
        return _body?.Tasks ?? Enumerable.Empty<PipelineTask>();
    }
}