using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Core.Approvals.Utils;

public static class MailChecker
{
    private const string MailRegEx = "^(?!(eu\\.|fu\\.|fu_))(.*)\\@rabobank\\.(.*)$";

    public static bool IsValidEmail(string email) =>
        Regex.IsMatch(email, MailRegEx, RegexOptions.IgnoreCase);
}