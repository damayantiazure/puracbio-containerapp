using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabobank.Compliancy.Clients.AzureDevopsClient.Helpers;

internal static class GuidExtensions
{
    public static Guid ToGuidOrDefault(this string value)
        => Guid.Parse(value);
}