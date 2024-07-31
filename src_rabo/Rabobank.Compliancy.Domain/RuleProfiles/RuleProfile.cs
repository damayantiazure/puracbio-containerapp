using Rabobank.Compliancy.Domain.Extensions;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Domain.RuleProfiles;

public abstract class RuleProfile : IEquatable<RuleProfile>
{
    public abstract IEnumerable<string> Rules { get; }
    public abstract Profiles Profile { get; }

    public string Name { get { return Enum.GetName(Profile); } }

    private static readonly Dictionary<Profiles, RuleProfile> _ruleProfileMapping = new()
    {
        { Profiles.Default, new DefaultRuleProfile() },
        { Profiles.MainframeCobol, new MainFrameCobolRuleProfile() }
    };

    public static RuleProfile GetProfile(string profileName)
    {
        var profile = GetValidProfileNameInternal(profileName);

        return _ruleProfileMapping.ContainsKey(profile) ? _ruleProfileMapping[profile] : new DefaultRuleProfile();
    }

    public static Profiles GetValidProfileName(string potentialProfilename)
    {
        return GetValidProfileNameInternal(potentialProfilename);
    }

    private static Profiles GetValidProfileNameInternal(string potentialProfilename)
    {
        return EnumHelper.ParseEnumOrDefault<Profiles>(potentialProfilename);
    }

    public virtual bool Equals([AllowNull] RuleProfile other)
    {
        if (other == null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return GetType() == other.GetType();
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as RuleProfile);
    }

    public override int GetHashCode()
    {
        return Rules.GetHashCode();
    }

}