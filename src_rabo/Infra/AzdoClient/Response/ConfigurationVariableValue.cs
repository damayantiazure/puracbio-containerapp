namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ConfigurationVariableValue
{
    public bool AllowOverride { get; set; }

    public bool IsSecret { get; set; }

    public string Value { get; set; }
}