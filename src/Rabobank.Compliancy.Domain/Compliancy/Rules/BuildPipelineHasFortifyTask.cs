using Rabobank.Compliancy.Domain.Compliancy.ExpectedTasks;
namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class BuildPipelineHasFortifyTask : PipelineHasTasksRule
{
    private static readonly IEnumerable<IExpectedTask> _expectedTasks = new List<IExpectedTask>
    {
        new FortifyTask()
    };

    public BuildPipelineHasFortifyTask() : base(_expectedTasks)
    {
    }
}