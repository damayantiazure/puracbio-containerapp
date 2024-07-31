namespace Rabobank.Compliancy.Domain.Compliancy.Authorizations;

public class Group : IIdentity
{
    private readonly List<IIdentity> _members = new();

    public string DisplayName { get; }

    public string UniqueId { get; }

    public Group(string displayName, string uniqueId)
    {
        DisplayName = displayName;
        UniqueId = uniqueId;
    }

    public IList<IIdentity> GetMembers()
    {
        return _members;
    }

    public IEnumerable<IIdentity> MapIdentitiesHierarchy()
    {
        var returnList = new HashSet<IIdentity>();
        foreach (var identity in _members)
        {
            returnList.UnionWith(identity.MapIdentitiesHierarchy());
        }
        returnList.Add(this);
        return returnList;
    }

    public void AddMember(IIdentity member)
    {
        _members.Add(member);
    }

    public void AddMembers(IEnumerable<IIdentity> members)
    {
        _members.AddRange(members);
    }

    public void RemoveMember(IIdentity member)
    {
        _members.Remove(member);
    }

    public void ClearMembers()
    {
        _members.Clear();
    }
}