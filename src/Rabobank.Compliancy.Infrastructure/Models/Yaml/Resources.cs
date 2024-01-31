namespace Rabobank.Compliancy.Infrastructure.Models.Yaml;

public class Resources
{
    public IEnumerable<BuildModel> Builds { get; set; }
    public IEnumerable<RepositoryModel> Repositories { get; set; }
    public IEnumerable<PipelineResource> Pipelines { get; set; }
}