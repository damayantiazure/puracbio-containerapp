using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Rabobank.Compliancy.Clients.AzureDataTablesClient;

public static class RowKeyGenerator
{
    public static readonly string NonProd = "NON-PROD";

    /// <summary>
    ///     Generates a RowKey in a uniform way
    /// </summary>
    public static string GenerateRowKey(params object?[] rowKeyParameters) =>
        CreateMd5(string.Join("_", rowKeyParameters));

    public static string GenerateVerySpecialRowKey(string? ciIdentifier, string projectId, string pipelineId,
        string pipelineType, string stageId) =>
        SanitizeKey($"{ciIdentifier ?? NonProd}|{projectId}|{pipelineId}|{pipelineType}|{stageId}");

    private static string SanitizeKey(string key)
    {
        var regEx = new Regex(@"[\\\\#%+ /?\u0000-\u001F\u007F-\u009F]");
        return regEx.IsMatch(key) ? regEx.Replace(key, string.Empty) : key;
    }

    private static string CreateMd5(string input) =>
        BitConverter.ToString(MD5.Create().ComputeHash(Encoding.ASCII.GetBytes(input)))
            .Replace("-", string.Empty)
            .ToLower();
}