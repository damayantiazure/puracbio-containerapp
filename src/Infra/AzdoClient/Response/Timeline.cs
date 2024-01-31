using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class Timeline
{
    public IEnumerable<TimelineRecord> Records { get; set; }
}