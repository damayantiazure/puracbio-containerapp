namespace Rabobank.Compliancy.Infrastructure.Tests.Helpers;

public static class ResourceFileHelper
{
    private static readonly string Resources = "Resources";

    internal static string GetContentFromResourceFile(string filename)
    {
        var resourceFilePath = Path.Combine(Resources, filename);
        if (!File.Exists(resourceFilePath))
        {
            return string.Empty;
        }

        return File.ReadAllText(resourceFilePath);
    }
}