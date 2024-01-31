using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

[ExcludeFromCodeCoverage]
public class ConfigurationItemModel
{
    public string? AssignmentGroup { get; set; }

    public string? ConfigurationItem { get; set; }

    [Newtonsoft.Json.JsonProperty(PropertyName = "rn.deployment.information")]
    public IEnumerable<DeploymentInfo>? DeploymentInfo { get; set; }

    //BIVcode = AIC rating
    public string? BIVcode { get; set; }
    public string? CiIdentifier { get; set; }
    public string? ConfigurationItemSubType { get; set; }
    public string? DisplayName { get; set; }
    public string? SOXClassification { get; set; }
    public string? Status { get; set; }
    public IEnumerable<string>? Environment { get; set; }
    public string? ConfigurationItemType { get; set; }        
}