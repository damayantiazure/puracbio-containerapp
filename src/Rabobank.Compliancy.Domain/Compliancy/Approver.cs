namespace Rabobank.Compliancy.Domain.Compliancy;

public class Approver
{
    public Guid Id { get; set; }

    public string DisplayName { get; set; }

    public string UniqueName { get; set; }

    public string Descriptor { get; set; }
}