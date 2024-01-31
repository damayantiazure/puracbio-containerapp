namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// This class represents an Application or IT-Service provided within and by Rabobank.
/// Any Application should be registered in a CMDB and have a unique identifier.
/// An Application is a component that we can report on.
/// </summary>
public class Application
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string AssignmentGroup { get; set; }
}