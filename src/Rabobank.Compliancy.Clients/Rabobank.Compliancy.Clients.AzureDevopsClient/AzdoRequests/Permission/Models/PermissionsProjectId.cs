namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models
{
    [Obsolete("This class is part of an undocumented legacy API.")]
    public class PermissionsProjectId
    {
        /// <summary>
        /// Getter and setter for the identity of the user.
        /// </summary>
        public Identity Identity { get; init; } = default!;

        public PermissionsSet? Security { get; set; }
    }
}