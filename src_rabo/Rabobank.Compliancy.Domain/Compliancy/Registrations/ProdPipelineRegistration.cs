namespace Rabobank.Compliancy.Domain.Compliancy.Registrations;

public class ProdPipelineRegistration : PipelineRegistration
{
    public string StageId { get; set; }
    public Application Application { get; set; }
}