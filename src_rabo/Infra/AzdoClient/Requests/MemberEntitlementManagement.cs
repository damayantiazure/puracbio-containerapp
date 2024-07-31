using Microsoft.AspNetCore.JsonPatch;
using Rabobank.Compliancy.Infra.AzdoClient.Response;
using System;
using System.Collections.Generic;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class MemberEntitlementManagement
{
    public static IAzdoRequest<JsonPatchDocument, OperationResults> PatchUserEntitlements(Guid entitlementId) =>
        new MemberEntitlementManagementRequest<JsonPatchDocument, OperationResults>($"_apis/UserEntitlements/{entitlementId}",
            new Dictionary<string, object>
            {
                ["api-version"] = "6.1-preview.3"
            },
            new Dictionary<string, object>
            {
                ["Content-Type"] = "application/json-patch+json"
            });

    public static MemberEntitlementManagementRequest<UserEntitlement> GetUserEntitlement(string entitlementId) =>
        new MemberEntitlementManagementRequest<UserEntitlement>(
            $"_apis/userentitlements/{entitlementId}", new Dictionary<string, object>
            {
                {"api-version", "6.1-preview.3"}
            });

    public static IEnumerableRequest<UserEntitlement> UserEntitlements() =>
        new MemberEntitlementManagementRequest<UserEntitlement>("_apis/UserEntitlements", new Dictionary<string, object>
        {
            ["api-version"] = "6.1-preview.3"
        }).AsEnumerable();

    public static IEnumerableRequest<UserEntitlement> UserEntitlements(string filter) =>
        new MemberEntitlementManagementRequest<UserEntitlement>("_apis/UserEntitlements", new Dictionary<string, object>
        {
            ["$filter"] = filter,
            ["api-version"] = "6.1-preview.3"
        }).AsEnumerable();

    public static MemberEntitlementManagementRequest<UserEntitlementSummary> UserEntitlementSummary() =>
        new MemberEntitlementManagementRequest<UserEntitlementSummary>("_apis/userentitlementsummary", new Dictionary<string, object>
        {
            ["api-version"] = "6.1-preview.1"
        });
}