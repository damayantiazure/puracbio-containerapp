using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Owner
{
    public int Id { get; set; }
    public string Name { get; set; }
        
    [JsonProperty(PropertyName = "_links")]
    public Links Links { get; set; }
}