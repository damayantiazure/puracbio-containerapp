using System;

namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class TimelineRecord
{
    public Guid Id { get; set; }
    public Guid? ParentId { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string Identifier { get; set; }
    public DateTime? StartTime { get; set; }
    public Result? Result { get; set; }
    public Log Log { get; set; }
}