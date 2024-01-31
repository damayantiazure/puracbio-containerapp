namespace Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

public class GateContainingEvaluatable : IEvaluatable
{
    private readonly PipelineBody _body = null!;

    public GateContainingEvaluatable(PipelineBody body)
    {
        _body = body ?? throw new ArgumentNullException(nameof(body));
    }

    public IEnumerable<Gate> GetGatesForStage(Stage prodStage)
    {
        var gates = _body.Stages
            .Where(stage => stage.Equals(prodStage))
            .SelectMany(stage => stage.Gates);

        return gates;
    }
}