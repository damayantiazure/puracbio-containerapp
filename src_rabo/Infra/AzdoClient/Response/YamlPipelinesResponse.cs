using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class YamlPipelinesResponse
{
    public IEnumerable<Pipeline> Value { get; set; }
}