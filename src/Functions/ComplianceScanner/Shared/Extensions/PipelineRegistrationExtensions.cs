using Rabobank.Compliancy.Infra.StorageClient.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rabobank.Compliancy.Functions.ComplianceScanner.Shared.Extensions;

public static class PipelineRegistrationExtensions
{
    public static string GetCiIdentifiersDisplayString(this IEnumerable<PipelineRegistration> registrations) 
    {
        if (!registrations.Any())
        {
            return string.Empty;
        }
        return registrations
            .Select(r => r.CiIdentifier)
            .Distinct()
            .Aggregate((a, b) => $"{a}, {b}");
    }

    public static string GetCiNamesDisplayString(this IEnumerable<PipelineRegistration> registrations)
    {
        if (!registrations.Any())
        {
            return string.Empty;
        }
        return registrations
            .Select(r => r.CiName)
            .Distinct()
            .Aggregate((a, b) => $"{a}, {b}");
    }   
}