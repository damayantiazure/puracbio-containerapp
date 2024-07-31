using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.StorageClient.Model;

public class PipelineRegistrationGroup
{
    public string ItemId { get; set; }
    public IList<PipelineRegistration> PipelineRegistrations { get; set; }
}