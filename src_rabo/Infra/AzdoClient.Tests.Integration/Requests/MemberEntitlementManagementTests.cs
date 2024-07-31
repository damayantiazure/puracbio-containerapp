using System;
using Microsoft.AspNetCore.JsonPatch;
using Rabobank.Compliancy.Infra.AzdoClient.Requests;
using Shouldly;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Rabobank.Compliancy.Infra.AzdoClient.Tests.Integration.Requests;

[Trait("category", "integration")]
public class MemberEntitlementManagementTests : IClassFixture<TestConfig>
{
    private readonly IAzdoRestClient _client;

    public MemberEntitlementManagementTests(TestConfig config) => 
        _client = new AzdoRestClient(config.Organization, config.Token);

    [Fact]
    public async Task TestUserEntitlement()
    {
        var result = (await _client.GetAsync(MemberEntitlementManagement.UserEntitlements())).ToList();

        var first = result.First(x => x.LastAccessedDate != default);
        first.DateCreated.ShouldNotBe(default);
        first.Id.ShouldNotBe(Guid.Empty);

        var user = first.User;
        user.PrincipalName.ShouldNotBe(default);
        user.MailAddress.ShouldNotBe(default);
        user.DisplayName.ShouldNotBe(default);

        var msdn = result.First(x => x.AccessLevel.LicensingSource == "msdn").AccessLevel;
        msdn.Status.ShouldNotBeEmpty();
        msdn.LicenseDisplayName.ShouldNotBeEmpty();
        msdn.MsdnLicenseType.ShouldNotBe("none");
        msdn.AccountLicenseType.ShouldBe("none");

        var account = result.First(x => x.AccessLevel.LicensingSource == "account").AccessLevel;
        account.Status.ShouldNotBeEmpty();
        account.MsdnLicenseType.ShouldBe("none");
        account.AccountLicenseType.ShouldNotBe("none");
        account.MsdnLicenseType.ShouldNotBeEmpty();
    }

    [Fact]
    public async Task TestMultipleEntitlements_WhenResultIsMoreThanTake_ThenRemainderShouldFetchedInSubsequentRequest()
    {
        var result = await _client.GetAsync(MemberEntitlementManagement.UserEntitlements());
        result.Count().ShouldBeGreaterThan(20);
    }

    [Fact]
    public async Task CanFilterUserEntitlements()
    {
        var filter = "licenseId eq 'Account-Unknown'";
        var result = await _client.GetAsync(MemberEntitlementManagement.UserEntitlements(filter));
        result.Count().ShouldBe(0);
    }

    [Theory]
    [InlineData("stakeholder")]
    [InlineData("express")]
    public async Task TestUpdateLicense(string license)
    {
        const string user = "eu.AzdoComplTest@rabobank.com";

        var entitlement = (await _client.GetAsync(MemberEntitlementManagement.UserEntitlements())).FirstOrDefault(e => e.User.MailAddress.Equals(user));

        entitlement!.AccessLevel.AccountLicenseType = license;

        var patchDocument = new JsonPatchDocument().Replace("/accessLevel", entitlement.AccessLevel);
        var patchResult = await _client.PatchAsync(MemberEntitlementManagement.PatchUserEntitlements(entitlement.Id), patchDocument);

        var result = (await _client.GetAsync(MemberEntitlementManagement.UserEntitlements())).FirstOrDefault(e => e.User.MailAddress.Equals(user));

        result!.AccessLevel.AccountLicenseType.ShouldBe(license);
        patchResult.Results.ShouldNotBeNull();
        patchResult.Results.ShouldNotBeEmpty();
        patchResult.Results.First().Errors.ShouldBeEmpty();
        patchResult.Results.First().UserId.ShouldNotBe(Guid.Empty);
        patchResult.Results.First().Result.ShouldNotBeNull();
        patchResult.Results.First().IsSuccess.ShouldBeTrue();
    }

    [Fact]
    public async Task TestUserEntitlementSummary()
    {
        var result = await _client.GetAsync(MemberEntitlementManagement.UserEntitlementSummary());
        result.Licenses.ShouldNotBeEmpty();

        var license = result.Licenses.First();
        license.LicenseName.ShouldNotBeEmpty();
        license.Assigned.ShouldNotBe(default);
    }

    [Fact]
    public async Task GetUserEntitlementShouldReturnUser()
    {
        // The provided Id below corresponds to 'eu.AzdoComplTest@rabobank.com' on Raboweb-Test
        var userEntitlement = await _client.GetAsync(
            MemberEntitlementManagement.GetUserEntitlement("85b560a6-5bc3-673b-8852-2c4eea8361b8"));
        userEntitlement.ShouldNotBeNull();
        userEntitlement.AccessLevel.ShouldNotBeNull();
    }

    [Fact]
    public async Task GetUserEntitlementForUnexistingUserReturnsNull()
    {
        var userEntitlement = await _client.GetAsync(
            MemberEntitlementManagement.GetUserEntitlement("8a0d9279-ab1c-655a-8cf6-538423527af1"));
        userEntitlement.ShouldBeNull();
    }
}