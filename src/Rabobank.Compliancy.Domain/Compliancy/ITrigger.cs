namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// Interface definition for describing a trigger
/// </summary>
public interface ITrigger
{
    public int Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Organization { get; set; }
}