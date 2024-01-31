namespace Rabobank.Compliancy.Domain.Compliancy;

public class PipelineTask
{
    public Guid? Id { get; set; }

    public string Name { get; set; }

    public string Content { get; set; }

    public IDictionary<string, string> Inputs { get; set; }
}