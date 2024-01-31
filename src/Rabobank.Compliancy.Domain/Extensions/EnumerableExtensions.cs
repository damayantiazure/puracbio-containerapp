namespace Rabobank.Compliancy.Domain.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TObject> EnsureNonEmptyCollection<TObject>(this IEnumerable<TObject> enumerable)
    {
        if (enumerable == null || !enumerable.Any())
        {
            throw new ArgumentException("Collection should not be null or empty", nameof(enumerable));
        }

        return enumerable;
    }
}