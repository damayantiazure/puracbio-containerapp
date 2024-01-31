using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using Microsoft.Azure.Cosmos.Table;
using Rabobank.Compliancy.Domain.RuleProfiles;
using Rabobank.Compliancy.Infra.Sm9Client.Cmdb.Model;

namespace Rabobank.Compliancy.Infra.StorageClient.Model;
public sealed class PipelineRegistration : TableEntity, IEquatable<PipelineRegistration>
{
    public static readonly string Prod = "PROD";
    public static readonly string NonProd = "NON-PROD";
    private string _ruleProfileName = nameof(Profiles.Default);

    public PipelineRegistration()
    {
    }

    public PipelineRegistration(ConfigurationItem configurationItem, string organization, string projectId,
        string pipelineId, string pipelineType, string stageId, string profile)
    {
        PartitionKey = CreatePartitionKey(configurationItem?.CiID);
        RowKey = CreateRowKey(configurationItem?.CiID, projectId, pipelineId, pipelineType, stageId);
        ProjectId = projectId;
        PipelineId = pipelineId;
        PipelineType = pipelineType;
        StageId = stageId;
        RuleProfileName = profile;
        CiIdentifier = configurationItem?.CiID;
        Organization = organization;
        CiName = configurationItem?.CiName;
        AssignmentGroup = configurationItem?.ConfigAdminGroup;
        IsSoxApplication = "Critical".Equals(configurationItem?.SOXClassification, StringComparison.OrdinalIgnoreCase);
        AicRating = configurationItem?.AicClassification;
        CiSubtype = configurationItem?.CiSubtype;
        ETag = "*";
    }

    [SuppressMessage("Sonar Code Smell",
        "S107: Constructor has 8 parameters, which is greater than the 7 authorized.",
        Justification = "This class will be phased out and refactored in the new architecture as a new class.")]
    public PipelineRegistration(ConfigurationItem configurationItem, string organization, string projectId,
        string pipelineId, string pipelineType, string stageId, string profile, bool? toBeScanned)
    {
        PartitionKey = CreatePartitionKey(configurationItem?.CiID);
        RowKey = CreateRowKey(configurationItem?.CiID, projectId, pipelineId, pipelineType, stageId);
        Organization = organization;
        ProjectId = projectId;
        CiIdentifier = configurationItem?.CiID;
        CiName = configurationItem?.CiName;
        CiSubtype = configurationItem?.CiSubtype;
        AssignmentGroup = configurationItem?.ConfigAdminGroup;
        AicRating = configurationItem?.AicClassification;
        PipelineId = pipelineId;
        PipelineType = pipelineType;
        StageId = stageId;
        RuleProfileName = profile;
        ToBeScanned = IsProduction ? null : toBeScanned;
    }

    public string Organization { get; set; }
    public string ProjectId { get; set; }
    public string PipelineId { get; set; }
    public string PipelineType { get; set; }
    public string StageId { get; set; }
    public bool? ToBeScanned { get; set; }
    public string CiIdentifier { get; set; }
    public string CiName { get; set; }
    public string CiSubtype { get; set; }
    public bool IsSoxApplication { get; set; }
    public string AssignmentGroup { get; set; }
    public string AicRating { get; set; }

    /// <summary>
    ///     Getter and setter for the name of the profile.
    /// </summary>
    public string RuleProfileName
    {
        get => _ruleProfileName;
        set
        {
            if (value != null && Enum.TryParse<Profiles>(value, out _))
            {
                _ruleProfileName = value;
            }
        }
    }

    public bool IsProduction => PartitionKey == Prod;

    public bool Equals([AllowNull] PipelineRegistration other)
    {
        //Check whether the compared object is null.
        if (other == null)
        {
            return false;
        }

        //Check whether the compared object references the same data.
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        //Check whether the products' properties are equal.
        return PartitionKey.Equals(other.PartitionKey) &&
               RowKey.Equals(other.RowKey);
    }

    public override bool Equals(object obj) => Equals(obj as PipelineRegistration);

    public override int GetHashCode() =>
        PartitionKey.GetHashCode() ^ RowKey.GetHashCode();

    public int? GetStageIdAsNullableInt()
    {
        if (StageId != null && PipelineType == "Classic release" && int.TryParse(StageId, out var id))
        {
            return id;
        }

        return null;
    }

    public RuleProfile GetRuleProfile() => RuleProfile.GetProfile(RuleProfileName);

    internal static string CreatePartitionKey(string ciIdentifier) =>
        string.IsNullOrEmpty(ciIdentifier) ? NonProd : Prod;

    internal static string CreateRowKey(string ciIdentifier, string projectId, string pipelineId, string pipelineType,
        string stageId) =>
        SanitizeKey($"{ciIdentifier ?? NonProd}|{projectId}|{pipelineId}|{pipelineType}|{stageId}");

    private static string SanitizeKey(string key)
    {
        var regEx = new Regex(@"[\\\\#%+ /?\u0000-\u001F\u007F-\u009F]");
        return regEx.IsMatch(key) ? regEx.Replace(key, string.Empty) : key;
    }
}