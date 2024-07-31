namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class Strategy
{
    public RunOnce RunOnce { get; set; }
    public Rolling Rolling { get; set; }
    public Canary Canary { get; set; }
}