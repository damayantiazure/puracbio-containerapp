using Newtonsoft.Json;
using Rabobank.Compliancy.Infrastructure.Models.Yaml;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Rabobank.Compliancy.Infrastructure.Parsers;

public static class YamlParser
{
    /// <summary>
    /// Will parse yaml content to a model <see cref="YamlModel"/> class.
    /// </summary>
    /// <param name="content">The yaml file content.</param>
    /// <returns>An instance of the parsed yaml file as <see cref="YamlModel"/>.</returns>
    public static YamlModel ParseToYamlModel(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return null;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        var dictionary = deserializer.Deserialize<Dictionary<string, object>>(content);

        var json = JsonConvert.SerializeObject(dictionary, Formatting.Indented);

        return JsonConvert.DeserializeObject<YamlModel>(json);
    }
}