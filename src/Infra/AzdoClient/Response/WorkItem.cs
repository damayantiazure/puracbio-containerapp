using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class WorkItem : WorkItemReference
{
    public IDictionary<string, object> Fields { get; set; }
}