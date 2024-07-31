using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class WorkItemQueryResult
{
    public DateTime AsOf { get; set; }
    public IList<WorkItemReference> WorkItems { get; set; }
}