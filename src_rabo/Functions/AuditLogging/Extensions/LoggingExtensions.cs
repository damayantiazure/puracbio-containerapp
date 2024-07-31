using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Functions.AuditLogging.Extensions;

public static class LoggingExtensions
{

    public static string RemoveUniversalDateTimeString(this string logrecord)
    {
        // Regex for detecting universal datetime format 0000-00-00T00:00:00.0000000Z
        var regex = new Regex(@"\b\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{7}Z\b.");
        return regex.Replace(logrecord, "");
    }

    public static string RemoveNewlines(this string logrecord)
    {
        return logrecord.Replace("\n", "").Replace("\r", "");
    }
}