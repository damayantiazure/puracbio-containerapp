using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class BuildPhase
{
    public IEnumerable<BuildStep> Steps { get; set; }
}