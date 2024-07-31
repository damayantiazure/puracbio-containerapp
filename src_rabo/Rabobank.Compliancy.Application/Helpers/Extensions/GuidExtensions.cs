namespace Rabobank.Compliancy.Application.Helpers.Extensions;

public static class GuidExtensions
{
    public static bool HasGuidValue(this string guidValue)
    {
        var guid = guidValue.ToGuidOrDefault();

        return !(guid == null || guid == Guid.Empty);
    }

    public static Guid? ToGuidOrDefault(this string guidValue)
    {
        return Guid.TryParse(guidValue, out var value) ? value : null;
    }
}