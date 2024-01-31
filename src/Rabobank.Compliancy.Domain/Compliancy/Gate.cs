namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// This class represents an entity which can contain one or more checks.
/// At the moment, this is only a azure function check.
/// </summary>
public class Gate
{
    public IEnumerable<AzureFunctionCheck> Checks { get; set; }
}