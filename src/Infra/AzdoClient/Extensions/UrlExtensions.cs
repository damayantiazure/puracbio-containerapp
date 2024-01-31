using System;
using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Infra.AzdoClient.Extensions;

public static class UrlExtensions
{
    public static string GetAzdoOrganizationName(this string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            throw new ArgumentNullException(nameof(url));
        }

        var regex = new Regex("^(https://)(vsrm.)?(dev.azure.com/)(?<organization>.*)(/)?$");
        return regex.Match(url)?.Groups["organization"]?.Value;
    }
}