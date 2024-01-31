using Rabobank.Compliancy.Functions.Sm9Changes.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Functions.Sm9Changes.Extensions;

public static class StringExtensions
{
    public static IEnumerable<string> GetChangeIdsFromTags(
        this IEnumerable<string> tags, string regex) =>
        tags
            .Where(t => t.IsValidChangeId(regex))
            .Select(t => t.GetChangeIdFromTag())
            .Distinct();

    private static string GetChangeIdFromTag(this string tag) =>
        Regex.Match(tag, SM9Constants.GetChangeIdRegex).Value;
        
    private static bool IsValidChangeId(this string changeId, string regex) =>
        !string.IsNullOrEmpty(changeId) && Regex.IsMatch(changeId, regex);

    public static bool IsValidChangeId(this string changeId) =>
        !string.IsNullOrEmpty(changeId) && Regex.IsMatch(changeId, SM9Constants.ChangeIdRegex);

    public static bool IsValidPipelineRunId(this string id) => 
        Regex.IsMatch(id, "^[\\d]+$");

    public static bool IsValidEmail(this string email) =>
        !string.IsNullOrEmpty(email) && Regex.IsMatch(email, SM9Constants.MailRegex, RegexOptions.IgnoreCase);

    public static bool IsLowRiskChange(this string value) =>
        string.Equals(value, SM9Constants.LowRiskChangeValue, StringComparison.InvariantCultureIgnoreCase);

    public static string ToCommaSeparatedString(this IEnumerable<string> values) =>
        string.Join(',', values);
}