namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class YamlModel
{
    public TriggerModel Trigger { get; set; }

    public Resources Resources { get; set; }

    public IEnumerable<StageModel> Stages { get; set; }

    public IEnumerable<JobModel> Jobs { get; set; }
}