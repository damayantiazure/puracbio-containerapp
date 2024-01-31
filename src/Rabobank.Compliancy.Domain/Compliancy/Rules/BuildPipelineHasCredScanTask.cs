using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class BuildPipelineHasCredScanTask : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask>
    {
        new CredScanTask(),
        new PostAnalysisTask()
    };

    public BuildPipelineHasCredScanTask() : base(_expectedTasks)
    {
    }
}