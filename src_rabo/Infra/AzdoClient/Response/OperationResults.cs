using Newtonsoft.Json;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class OperationResults
{
    [JsonProperty(PropertyName = "operationResults")]
    public IEnumerable<OperationResult> Results { get; set; }
}