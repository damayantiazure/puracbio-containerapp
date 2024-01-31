namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class TaskNameExtensions
{
    internal static string StripNamespaceAndVersion(this string fullTaskName)
    {
        if (fullTaskName == null)
        {
            return string.Empty;
        }

        var taskWithoutVersion = fullTaskName.Split("@").First();
        return taskWithoutVersion.Split(".").Last();
    }
}