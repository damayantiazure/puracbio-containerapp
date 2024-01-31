using System;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Application;

public class CloseChangeRequest
{
    public string Organization { get; set; }
    public Guid ProjectId { get; set; }
    public string PipelineType { get; set; }
    public int RunId { get; set; }
    public CloseChangeDetails CloseChangeDetails { get; set; }
}