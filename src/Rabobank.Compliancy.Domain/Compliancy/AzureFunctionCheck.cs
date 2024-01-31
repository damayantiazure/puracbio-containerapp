namespace Rabobank.Compliancy.Domain.Compliancy;

/// <summary>
/// This class represents a check that can be added to an environment or a gate in a classic pipeline
/// </summary>
public class AzureFunctionCheck : Check
{
    public string Method { get; set; }
    public string Function { get; set; }
    public bool WaitForCompletion { get; set; }
}