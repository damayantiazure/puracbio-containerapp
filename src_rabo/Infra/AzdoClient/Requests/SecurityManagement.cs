using System.Collections.Generic;
using Newtonsoft.Json;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class SecurityManagement
{
    public static IAzdoRequest<Response.Security.IdentityGroup> GroupMembers(string projectId, string groupId) =>
        new AzdoRequest<Response.Security.IdentityGroup>($"/{projectId}/_api/_identity/ReadGroupMembers",
            new Dictionary<string, object>
            {
                {"__v", "5"},
                {"scope", groupId},
                {"readMembers", "true" }
            });

    public static IAzdoRequest<Response.Security.IdentityGroup> Groups(string projectId) =>
        new AzdoRequest<Response.Security.IdentityGroup>(
            $"/{projectId}/_api/_identity/ReadScopedApplicationGroupsJson",
            new Dictionary<string, object>
            {
                {"__v", "5"}
            });

    public static IAzdoRequest<AddMemberData, object> AddMember(string project) =>
        new AzdoRequest<AddMemberData, object>($"/{project}/_api/_identity/AddIdentities",
            new Dictionary<string, object>
            {
                {"__v", "5"}
            });


    public static IAzdoRequest<EditMembersData, object> EditMembership(string project) =>
        new AzdoRequest<EditMembersData, object>($"/{project}/_api/_identity/EditMembership",
            new Dictionary<string, object>
            {
                {"__v", "5"}
            });


    public class EditMembersData
    {
        protected EditMembersData(string groupId)
        {
            GroupId = groupId;
        }
        public bool EditMembers { get; } = true;
        public string GroupId { get; }
    }

    public class RemoveMembersData : EditMembersData
    {
        public RemoveMembersData(IEnumerable<string> users, string group) : base(group)
        {
            RemoveItemsJson = JsonConvert.SerializeObject(users);
        }

        public string RemoveItemsJson { get; }
    }

    public class AddMemberData
    {
        public string ExistingUsersJson { get; }
        public string GroupsToJoinJson { get; }

        public AddMemberData(IEnumerable<string> users, IEnumerable<string> groups)
        {
            ExistingUsersJson = JsonConvert.SerializeObject(users);
            GroupsToJoinJson = JsonConvert.SerializeObject(groups);
        }
    }

    public static IAzdoRequest<ManageGroupData, Response.ApplicationGroup> ManageGroup(string project) =>
        new AzdoRequest<ManageGroupData, Response.ApplicationGroup>($"/{project}/_api/_identity/ManageGroup",
            new Dictionary<string, object>
            {
                {"__v", "5"}
            });


    public class ManageGroupData
    {
        public string Name { get; set; }
    }

    public static IAzdoRequest<string, object> DeleteIdentity(string project) =>
        new AzdoRequest<string, object>($"/{project}/_api/_identity/DeleteIdentity",
            new Dictionary<string, object>
            {
                {"__v", "5"}
            });

}