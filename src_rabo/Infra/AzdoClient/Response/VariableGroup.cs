using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class VariableGroup
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Type { get; set; }
    public IDictionary<string, VariableValue> Variables { get; set; }
}