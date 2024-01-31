using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

namespace Rabobank.Compliancy.Domain.Compliancy.Rules;

public abstract class NobodyCanMisuseObject<TEnum> : IRule<IIdentityPermissionEvaluatable<TEnum>> where TEnum : struct
{
    protected abstract IEnumerable<TEnum> MisUseTypes { get; }

    public EvaluationResult Evaluate(IIdentityPermissionEvaluatable<TEnum> evaluatable)
    {
        if (evaluatable == null)
        {
            throw new ArgumentNullException(nameof(evaluatable));
        }

        var result = false;
        foreach (var misUseType in MisUseTypes)
        {
            // if the allowedIdentities is empty, we know there's no-one who can misuse the object, so we can set it to true and continue to the next
            if (!evaluatable.Permissions.ContainsKey(misUseType) || !evaluatable.Permissions.Any())
            {
                result = true;
                continue;
            }

            result = !AtLeastOneUserIsAllowedToMisUse(evaluatable.Permissions[misUseType]);

            // if at any point the result comes back as false, we can stop because we know someone is able to misuse the object
            if (!result)
            {
                break;
            }
        }
        return new EvaluationResult { Passed = result };
    }

    private bool AtLeastOneUserIsAllowedToMisUse(IEnumerable<IIdentity> allowedIdentities)
    {
        foreach (var subIdentity in allowedIdentities)
        {
            if ((subIdentity is Authorizations.User) || AtLeastOneUserIsAllowedToMisUse(((Group)subIdentity).GetMembers()))
            {
                return true;
            }
        }

        return false;
    }
}