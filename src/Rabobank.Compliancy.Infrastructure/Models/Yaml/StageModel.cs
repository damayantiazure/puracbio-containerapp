namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class StageModel
{
    public string Stage { get; set; }

    public string DisplayName { get; set; }

    public IEnumerable<JobModel> Jobs { get; set; }
}