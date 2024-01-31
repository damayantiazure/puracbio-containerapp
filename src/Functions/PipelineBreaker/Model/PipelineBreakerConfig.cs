namespace Rabobank.Compliancy.Functions.PipelineBreaker.Model;

public class PipelineBreakerConfig
{
    public bool BlockUnregisteredPipelinesEnabled { get; set; }
    public bool BlockIncompliantPipelinesEnabled { get; set; }
    public bool ThrowWarningsIncompliantPipelinesEnabled { get; set; }
}