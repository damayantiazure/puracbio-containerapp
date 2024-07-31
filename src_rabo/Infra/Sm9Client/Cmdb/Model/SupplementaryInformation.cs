using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

/// <summary>
/// Represents the registration information as it is recorded in the Cmdb. 
/// Can only be instantiated with a valid json string
/// </summary>
public class SupplementaryInformation
{
    private readonly string _cmdbJsonString;
    private static readonly JsonSerializerSettings _serializerSettings = new()
    {
        ContractResolver = new DefaultContractResolver
        {
            NamingStrategy = new CamelCaseNamingStrategy()
        }
    };

    private SupplementaryInformation(string jsonString, string organization, string project, string pipeline, string stage, string? profile)
    {
        _cmdbJsonString = jsonString;
        // These Properties should never be empty as they make up the core of what constitutes a valid registration in CMDB
        Organization = organization ?? throw new ArgumentNullException(nameof(organization));
        Project = project ?? throw new ArgumentNullException(nameof(project));
        Pipeline = pipeline ?? throw new ArgumentNullException(nameof(pipeline));
        Stage = stage ?? throw new ArgumentNullException(nameof(stage));
        // These Properties have been added later and can be optionally present
        Profile = profile;
    }

    public string Organization { get; private set; }
    public string Project { get; private set; }
    public string Pipeline { get; private set; }
    public string Stage { get; private set; }
    public string? Profile { get; private set; }

    /// <summary>
    /// Returns the json string as it was originally provided at the instantiation of the Class
    /// </summary>
    /// <returns></returns>
    public override string ToString() =>
        _cmdbJsonString;

    /// <summary>
    /// This static method allows a consistent conversion of the json string representing a registration in the cmdb.
    /// It will only return a instance of the class when the minimum requirements are met.
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
    public static SupplementaryInformation? ParseSupplementaryInfo(string? json)
    {
        try
        {
            return string.IsNullOrWhiteSpace(json)
                ? null
                : ParseInternal(json);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentNullException)
        {
            return null;
        }
    }

    private static SupplementaryInformation ParseInternal(string json)
    {
        var deserializedJson = JsonConvert.DeserializeObject<dynamic>(json, _serializerSettings)!;
        return new SupplementaryInformation(
            json,
            (string)deserializedJson.organization,
            (string)deserializedJson.project,
            (string)deserializedJson.pipeline,
            (string)deserializedJson.stage,
            (string)deserializedJson.profile);
    }
}