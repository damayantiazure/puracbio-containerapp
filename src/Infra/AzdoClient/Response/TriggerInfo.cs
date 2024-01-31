using System;
using System.Collections.Generic;
using System.Text;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class TriggerInfo
{
    public string ArtifactType { get; set; }
    public string PipelineId { get; set; }
    public string ProjectId { get; set; }
}