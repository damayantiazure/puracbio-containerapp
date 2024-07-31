namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public abstract class StepsContainer
{
    public IEnumerable<StepModel> Steps { get; set; }
}