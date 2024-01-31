using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class TaskGroup
{
    public IEnumerable<BuildStep> Tasks { get; set; }
}