using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers;
using Rabobank.Compliancy.Clients.AzureDevopsClient.Repositories.Interfaces;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Tests.PermissionsHelpers;

public class RecursiveIdentityCacheBuilderTests
{
    private readonly Mock<IIdentityRepository> _identityRepository = new();
    [Fact]
    public async Task GetIdentitiesFromCacheAsync_WithIdentitiesInMultipleGroups_OnlyUsesRepositoryOnceForEachIdentity()
    {
        // Arrange
        var usedDescriptors = new List<IEnumerable<IdentityDescriptor>>();
        _identityRepository.Setup(i => i.GetIdentitiesForIdentityDescriptorsAsync(It.IsAny<string>(), It.IsAny<IEnumerable<IdentityDescriptor>>(),
            QueryMembership.Direct, It.IsAny<CancellationToken>())).Returns(MockAzdoIdentities).Callback((string org, IEnumerable<IdentityDescriptor> descriptors, QueryMembership mem, CancellationToken can) => { usedDescriptors.Add(descriptors.ToArray()); });
        // Act

        var actual = await new RecursiveIdentityCacheBuilder(_identityRepository.Object).GetIdentitiesFromCacheAsync(string.Empty, new[] { group1.Descriptor }, default);

        // Assert
        usedDescriptors.Should().HaveCount(3);
        usedDescriptors.Where(u => u.Any(i => i.ToString() == user2.Descriptor.ToString())).Should().HaveCount(1);
        usedDescriptors.Where(u => u.Any(i => i.ToString() == user4.Descriptor.ToString())).Should().HaveCount(1);
    }

    private static readonly Identity group1 = new()
    {
        Descriptor = new("Group", "1"),
        IsContainer = true,
        Members = new IdentityDescriptor[]
        {
            new("Group", "2"),
            new("Group", "3")
        }
    };

    private static readonly Identity group2 = new()
    {
        Descriptor = new("Group", "2"),
        IsContainer = true,
        Members = new IdentityDescriptor[]
        {
            new("User", "1"),
            new("User", "2")
        }
    };

    private static readonly Identity group3 = new()
    {
        Descriptor = new("Group", "3"),
        IsContainer = true,
        Members = new IdentityDescriptor[]
        {
            new("Group", "4"),
            new("User", "2"),
            new("User", "3"),
            new("User", "4")
        }
    };

    private static readonly Identity group4 = new()
    {
        Descriptor = new("Group", "4"),
        IsContainer = true,
        Members = new IdentityDescriptor[]
        {
            new("User", "4")
        }
    };

    private static readonly Identity user1 = new() { Descriptor = new("User", "1") };
    private static readonly Identity user2 = new() { Descriptor = new("User", "2") };
    private static readonly Identity user3 = new() { Descriptor = new("User", "3") };
    private static readonly Identity user4 = new() { Descriptor = new("User", "4") };
    private static readonly Identity user5 = new() { Descriptor = new("User", "5") };

    private static readonly Dictionary<IdentityDescriptor, Identity> identityDatabase = new()
    {
        { group1.Descriptor, group1 },
        { group2.Descriptor, group2 },
        { group3.Descriptor, group3 },
        { group4.Descriptor, group4 },
        { user1.Descriptor, user1 },
        { user2.Descriptor, user2 },
        { user3.Descriptor, user3 },
        { user4.Descriptor, user4 },
        { user5.Descriptor, user5 }
    };

    private async Task<IEnumerable<Identity>> MockAzdoIdentities(string organization, IEnumerable<IdentityDescriptor> descriptors, QueryMembership queryMembership, CancellationToken cancellation)
    {
        return await Task.Run(() =>
        {
            var returnList = new List<Identity>();

            foreach (var descriptor in descriptors)
            {
                returnList.Add(identityDatabase[descriptor]);
            }

            return returnList;
        });
    }
}