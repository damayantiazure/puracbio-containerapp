using Rabobank.Compliancy.Domain.Compliancy.Evaluatables;

namespace Rabobank.Compliancy.Domain.Compliancy.Authorizations;

/// <summary>
/// Interface for an object that can have permissions assigned to it
/// Permissions are assigned to <see cref="IIdentity"/> objects
/// </summary>
public interface IProtectedObject<TPermissionEnum> : IEvaluatable where TPermissionEnum : Enum
{
    /// <summary>
    /// Provides a set of <see cref="IIdentity"/> that are able to delete this object or important components therof
    /// </summary>
    IDictionary<TPermissionEnum, IEnumerable<IIdentity>> Permissions { get; }
}