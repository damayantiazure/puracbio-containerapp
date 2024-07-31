using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class TaskGroupResponse
{
    public IEnumerable<TaskGroup> Value { get; set; }
}