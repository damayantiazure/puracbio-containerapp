using Rabobank.Compliancy.Domain.Compliancy;

namespace Rabobank.Compliancy.Infrastructure;

/// <summary>
/// Objectmodel for representing a pipelinetrigger.
/// A pipeline can be triggerd by a different pipeline, which is represented by an id and resides in a 
/// project in an specific organization
/// </summary>
public class PipelineTrigger : ITrigger
{
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Organization { get; set; }
}