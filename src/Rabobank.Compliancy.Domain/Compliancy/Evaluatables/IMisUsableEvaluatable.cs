using Rabobank.Compliancy.Domain.Compliancy.Authorizations;

namespace Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

/// <summary>
/// Represents an evaluatable that can be used in a non-compliant way if the permissions allow it. 
/// Rules will evaluate these permissions by checking if there are any identities (groups or users)
/// present in the IdentitiesThatCanMisuseThisEvaluatable property that are not in the DeniedIdentities.
/// </summary>
public interface IIdentityPermissionEvaluatable<TEnum> : IEvaluatable where TEnum : struct
{
    /// <summary>
    /// These are the identities that have the rights to misuse this evaluatable.
    /// </summary>
    public Dictionary<TEnum, IEnumerable<IIdentity>> Permissions { get; set; }
}