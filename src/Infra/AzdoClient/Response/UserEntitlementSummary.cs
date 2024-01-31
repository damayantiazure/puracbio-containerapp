using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class UserEntitlementSummary
{
    public IEnumerable<LicenseSummary> Licenses { get; set; }
}