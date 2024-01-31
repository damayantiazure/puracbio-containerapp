using FluentAssertions;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.Identity;
using Rabobank.Compliancy.Clients.AzureDevopsClient.PermissionsHelpers.Interfaces;
using Rabobank.Compliancy.Domain.Compliancy.Authorizations;
using Rabobank.Compliancy.Infrastructure.Parsers;

namespace Rabobank.Compliancy.Infrastructure.Tests.Parsers;

public class IdentityParserTests
{
    private readonly Mock<IRecursiveIdentityCacheBuilder> _cacheBuilder = new();
    private readonly IList<IdentityDescriptor> _identityDescriptorsToQuery = new List<IdentityDescriptor>();
    private readonly IList<Identity> _identitiesToReturn = new List<Identity>();
    private readonly IFixture _fixture = new Fixture();
    private readonly string _organization;
    private readonly IdentityParser _sut;

    public IdentityParserTests()
    {
        _organization = _fixture.Create<string>();
        _sut = new(_cacheBuilder.Object);
    }

    [Fact]
    public void ConstructParser_WithNullCache_Throws()
    {
        // Act
        var act = () => new IdentityParser(null);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithNullIdentityDescriptors_ReturnsEmptyEnumerable()
    {
        // Arrange
        IEnumerable<IdentityDescriptor>? descriptors = null;

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, descriptors);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithEmptyIdentityDescriptors_ReturnsEmptyEnumerable()
    {
        // Arrange
        var descriptors = Enumerable.Empty<IdentityDescriptor>();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, descriptors);

        // Assert
        result.Should().NotBeNull().And.BeEmpty();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithSingleGenericUserDescriptor_ReturnsUser()
    {
        // Arrange
        var displayName = _fixture.Create<string>();
        var descriptor = CreateGenericUserDescriptorAndAddToMockList(displayName);
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var firstResult = result.FirstOrDefault();

        // Assert
        result.Should().NotBeEmpty();
        firstResult?.Should().BeOfType<User>();
        firstResult?.UniqueId.Should().Be(descriptor.ToString());
        firstResult?.DisplayName.Should().Be(displayName);
        _cacheBuilder.Verify();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithSingleClaimsUserDescriptor_ReturnsUser()
    {
        // Arrange
        var descriptor = CreateClaimsUserDescriptorAndAddToMockList(_fixture.Create<string>(), _fixture.Create<string>());
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var firstResult = result.FirstOrDefault();

        // Assert
        result.Should().NotBeEmpty();
        firstResult.Should().BeOfType<User>();
        firstResult?.UniqueId.Should().Be(descriptor.ToString());
        _cacheBuilder.Verify();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithSingleClaimsUserDescriptor_ReturningIdentityWithAccountNotEmpty_ReturnsUserWithEmailAsDisplayName()
    {
        // Arrange
        var email = "MyEmail@domain.com";
        var descriptor = CreateClaimsUserDescriptorAndAddToMockList(_fixture.Create<string>(), email);
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var firstResult = result.FirstOrDefault();

        // Assert
        firstResult?.DisplayName.Should().Be(email);
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithSingleClaimsUserDescriptor_ReturningIdentityWithAccountEmpty_ReturnsUserWithDisplayNameAsDisplayName()
    {
        // Arrange
        var displayName = "My Name";
        var descriptor = CreateClaimsUserDescriptorAndAddToMockList(displayName, "");
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var firstResult = result.FirstOrDefault();

        // Assert
        firstResult?.DisplayName.Should().Be(displayName);
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithSingleGroupDescriptor_ReturnsGroup()
    {
        // Arrange
        var displayName = "My Group Name";
        var descriptor = CreateGroupDescriptorAndAddToMockList(displayName);
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var firstResult = result.FirstOrDefault();

        // Assert
        result.Should().NotBeEmpty();
        firstResult?.Should().NotBeNull().And.BeOfType<Group>();
        firstResult?.UniqueId.Should().Be(descriptor.ToString());
        firstResult?.DisplayName.Should().Be(displayName);
        _cacheBuilder.Verify();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithMultipleDescriptor_ReturnsMultiple()
    {
        // Arrange
        var descriptors = new[]
        {
            CreateGroupDescriptorAndAddToMockList(_fixture.Create<string>()),
            CreateGenericUserDescriptorAndAddToMockList(_fixture.Create<string>())
        };
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, descriptors);

        // Assert
        result.Should().HaveCount(2);
        _cacheBuilder.Verify();
    }

    [Fact]
    public async Task ParseIdentityDescriptors_WithGroupDescriptor_OfGroupWithMembers_ShouldReturnGroupWithMembers()
    {
        // Arrange
        var members = new[]
        {
            CreateGroupDescriptorAndAddToMockList(_fixture.Create<string>()),
            CreateGenericUserDescriptorAndAddToMockList(_fixture.Create<string>())
        };
        SetupMockAndClearMockLists();

        var descriptor = CreateGroupDescriptorAndAddToMockList(_fixture.Create<string>(), members);
        SetupMockAndClearMockLists();

        // Act
        var result = await _sut.ParseIdentityDescriptors(_organization, new[] { descriptor });
        var returnedmembers = result.FirstOrDefault()?.As<Group>()?.GetMembers();

        // Assert
        result.Should().NotBeEmpty().And.HaveCount(1);
        returnedmembers?.Count.Should().Be(2);
        _cacheBuilder.Verify();
    }

    private IdentityDescriptor CreateGroupDescriptorAndAddToMockList(string displayName)
    {
        return CreateGroupDescriptorAndAddToMockList(displayName, Enumerable.Empty<IdentityDescriptor>());
    }

    private IdentityDescriptor CreateGroupDescriptorAndAddToMockList(string displayName, IEnumerable<IdentityDescriptor> members)
    {
        var descriptor = new IdentityDescriptor(IdentityConstants.TeamFoundationType, _fixture.Create<string>());
        var identity = new Identity
        {
            Descriptor = descriptor,
            ProviderDisplayName = displayName,
            Members = members.ToList()
        };
        AddToMockList(descriptor, identity);

        return descriptor;
    }

    private IdentityDescriptor CreateClaimsUserDescriptorAndAddToMockList(string displayName, string email)
    {
        var descriptor = new IdentityDescriptor(IdentityConstants.ClaimsType, _fixture.Create<string>());
        var identity = new Identity
        {
            Descriptor = descriptor,
            ProviderDisplayName = displayName,
        };
        identity.SetProperty("Account", email);

        AddToMockList(descriptor, identity);

        return descriptor;
    }

    private IdentityDescriptor CreateGenericUserDescriptorAndAddToMockList(string displayName, string identityType = IdentityConstants.ServiceIdentityType)
    {
        var descriptor = new IdentityDescriptor(identityType, _fixture.Create<string>());
        var identity = new Identity
        {
            Descriptor = descriptor,
            ProviderDisplayName = displayName,
        };

        AddToMockList(descriptor, identity);

        return descriptor;
    }

    private void AddToMockList(IdentityDescriptor descriptor, Identity identity)
    {
        _identityDescriptorsToQuery.Add(descriptor);
        _identitiesToReturn.Add(identity);
    }

    private void SetupMockAndClearMockLists()
    {
        // Create new mock in- and output set from collected values
        var descriptors = new List<IdentityDescriptor>(_identityDescriptorsToQuery);
        var identities = new List<Identity>(_identitiesToReturn);

        // Set up mock
        _cacheBuilder.Setup(cachebuilder => cachebuilder.GetIdentitiesFromCacheAsync(_organization, descriptors, It.IsAny<CancellationToken>()))
            .ReturnsAsync(identities)
            .Verifiable();

        // Clear collected values for next mock setup
        _identityDescriptorsToQuery.Clear();
        _identitiesToReturn.Clear();
    }
}