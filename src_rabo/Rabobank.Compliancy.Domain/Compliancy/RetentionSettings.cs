#nullable enable

namespace Rabobank.Compliancy.Domain.Compliancy;

public class RetentionSettings : ISettings
{
    public int DaysToKeepRuns { get; set; }
}