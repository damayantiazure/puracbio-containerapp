using Rabobank.Compliancy.Domain.Enums;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Pipeline
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Folder { get; set; }
    public Project ParentProject { get; set; }
    public PipelineProcessType ProcessType { get; set; }
}