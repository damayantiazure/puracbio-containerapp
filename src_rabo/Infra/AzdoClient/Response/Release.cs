#nullable enable

using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Release
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public ReleaseDefinition? ReleaseDefinition { get; set; }
    public IEnumerable<Environment>? Environments { get; set; }
    public IEnumerable<ArtifactReference>? Artifacts { get; set; }
    public IEnumerable<string>? Tags { get; set; }
    public string? ReleaseDefinitionRevision { get; set; }
    public IdentityRef? CreatedBy { get; set; }
    public IdentityRef? CreatedFor { get; set; }
    public IDictionary<string, VariableValue>? Variables { get; set; }

    [JsonProperty("_links")]
    public Links? Links { get; set; }
}