using System;
using System.Collections.Generic;
using Rabobank.Compliancy.Infra.AzdoClient.Enumerators;
using Rabobank.Compliancy.Infra.AzdoClient.Response;

namespace Rabobank.Compliancy.Infra.AzdoClient.Requests;

public static class AuditLog
{
    public static IEnumerableRequest<AuditLogEntry> Query(DateTime? start = null, DateTime? end = null)
    {
        return new EnumerableRequest<AuditLogEntry, AuditLogEnumerator>(new AuditRequest<AuditLogEntry>("_apis/audit/auditLog", new Dictionary<string, object>
        {
            ["startTime"] = start,
            ["endTime"] = end
        }));
    }
}