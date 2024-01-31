using Newtonsoft.Json.Linq;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Core.Rules.Tests.Objects;

public class EnvironmentMock
{
    public EnvironmentYaml EnvironmentYaml { get; set; }
    public JObject EnvironmentCheck { get; set; }

    public EnvironmentMock() { }

    public EnvironmentMock(EnvironmentYaml environmentYaml, JObject environmentCheck)
    {
        EnvironmentYaml = environmentYaml;
        EnvironmentCheck = environmentCheck;
    }
}