namespace Rabobank.Compliancy.Infra.AzdoClient.Response;

public class ReleaseDefinitionGatesOptions
{
    public bool IsEnabled { get; set; }

    public int MinimumSuccessDuration { get; set; }

    public int SamplingInterval { get; set; }

    public int StabilizationTime { get; set; }

    public int Timeout { get; set; }
}