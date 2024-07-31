#nullable enable

using Rabobank.Compliancy.Domain.Compliancy;
using System.Diagnostics.CodeAnalysis;

namespace Rabobank.Compliancy.Application.Requests.OpenPermissions;

[SuppressMessage("Major Code Smell", "S2326:Unused type parameters should be removed", Justification = "Generic Type is used for downstream type constraints")]
public abstract class OpenPermissionsRequestBase<TProtectedResource> : RequestBase
    where TProtectedResource : IProtectedResource
{
}