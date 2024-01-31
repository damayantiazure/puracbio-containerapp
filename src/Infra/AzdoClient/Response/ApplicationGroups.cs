using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ApplicationGroups
{
    public IEnumerable<ApplicationGroup> Identities { get; set; }

}