namespace Rabobank.Compliancy.Infra.StorageClient.Model;

public class GetPipelineRegistrationRequest
{
    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string PipelineId { get; set; }
    public string PipelineType { get; set; }
}