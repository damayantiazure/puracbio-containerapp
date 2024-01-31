#nullable enable

using System;
using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Functions.AuditLogging.Helpers;

public static class TagExtensions
{
    private const string _changeIdRegex = "C[0-9]{9}";
    private const string _changeHashRegex = @"\[(.{7,8})\]";

    public static bool IsChangeTag(this string tag) =>
        new Regex(_changeIdRegex).IsMatch(tag);

    public static string? ChangeId(this string? tag) =>
        tag == null ? null : Regex.Match(tag, _changeIdRegex).Value;

    public static Uri? ChangeUrl(this string? tag)
    {
        if (tag == null)
        {
            return null;
        }

        var changeId = tag.ChangeId();
        var changeHash = Regex.Match(tag, _changeHashRegex).Groups[1].Value;

        if (string.IsNullOrEmpty(changeId) || string.IsNullOrEmpty(changeHash))
        {
            return null;
        }

        return new Uri($"http://itsm.rabobank.nl/SM/index.do?ctx=docEngine&file=cm3r&query=number%3D%22" +
                       $"{changeId}%22&action=&title=Change%20Request%20Details&queryHash={changeHash}");
    }
}