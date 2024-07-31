using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class BuildProcess
{
    public IEnumerable<BuildPhase> Phases { get; set; }

    public int Type { get; set; }

    public string YamlFilename { get; set; }
}