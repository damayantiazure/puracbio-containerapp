using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

namespace Rabobank.Compliancy.Tests.Helpers;

public static class AuthenticationExtensions
{
    public static string ToHashedToken(this string token) =>
        Encoding.Default.GetString(SHA256.HashData(Encoding.Default.GetBytes(token)));

    public static AuthenticationHeaderValue ToBasicAuthenticationHeader(this string token) =>
        new("Basic", Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", "", token))));
}