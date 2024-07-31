namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

public class ConfigurationItem
{
    public string? AicClassification { get; set; }
    public string? CiID { get; set; }
    public string? CiName { get; set; }
    public string? CiSubtype { get; set; }
    public string? CiType { get; set; }
    public string? ConfigAdminGroup { get; set; }
    public IEnumerable<string>? Environment { get; set; }
    public string? SOXClassification { get; set; }
    public string? Status { get; set; }

    public bool IsTypeValid =>
        CiType == "application" || CiType == "subapplication";

    public bool IsEnvironmentValid =>
        Environment != null && Environment.Contains("Production");

    public bool IsCiStatusValid =>
        !string.IsNullOrEmpty(Status) && Status.StartsWith("In Use - ", StringComparison.CurrentCulture);
}