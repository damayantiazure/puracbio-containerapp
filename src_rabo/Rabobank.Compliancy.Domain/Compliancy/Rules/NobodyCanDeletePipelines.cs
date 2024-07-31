using Rabobank.Compliancy.Domain.Compliancy.Evaluatables.MisUsableEvaluatableTypes;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public class NobodyCanDeletePipelines : NobodyCanMisuseObject<PipelineMisUse>
{
    protected override IEnumerable<PipelineMisUse> MisUseTypes
    {
        get
        {
            return new[]
            {
                PipelineMisUse.DeletePipelines,
                PipelineMisUse.DeleteRuns,
                PipelineMisUse.GrantPermissionsToSelf
            };
        }
    }
}