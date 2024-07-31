using System.Text.Json;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.AzdoRequests.Permission.Models;

[Obsolete("This class is part of an undocumented legacy API.")]
public class AddMemberData
{
    public string? ExistingUsersJson { get; }
    public string? GroupsToJoinJson { get; }

    public AddMemberData(IEnumerable<Guid> users, IEnumerable<Guid> groups)
    {
        ExistingUsersJson = JsonSerializer.Serialize(users);
        GroupsToJoinJson = JsonSerializer.Serialize(groups);
    }
}