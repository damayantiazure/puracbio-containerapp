#nullable enable

namespace Rabobank.Compliancy.Infrastructure.Extensions;

internal static class ListExtensions
{
    public static void Replace<T>(this IList<T> source, T oldValue, T newValue)
    {
        if (source == null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var index = source.IndexOf(oldValue);
        if (index == -1)
        {
            throw new InvalidOperationException("Item not found, unable to replace.");
        }

        source[index] = newValue;
    }
}
