using Microsoft.VisualStudio.Services.MemberEntitlementManagement.WebApi;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

/// <summary>
/// An interface defining the logic to communicate with the azure devops user entitlement enpoint.
/// </summary>
public interface IUserEntitlementRepository
{
    /// <summary>
    /// GetUserEntitlementByIdAsync will retrieve the user details from the azure devops endpoint.
    /// </summary>
    /// <param name="organization">The name of the organization.</param>
    /// <param name="userId">The user identifier.</param>
    /// <param name="cancellationToken">Propagates notification that operations should be canceled.</param>
    /// <returns>A nullable instance of the <see cref="UserEntitlement"/> class.</returns>
    Task<UserEntitlement?> GetUserEntitlementByIdAsync(string organization, Guid userId, CancellationToken cancellationToken = default);
}