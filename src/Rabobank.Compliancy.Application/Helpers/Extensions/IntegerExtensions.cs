namespace Rabobank.Compliancy.Application.Helpers.Extensions;

public static class IntegerExtensions
{
    private const int Zero = 0;
    public static bool HasIntegerValue(this string integerValue)
    {
        var integer = integerValue.ToIntegerOrDefault();

        return !(integer == null || integer.Value <= Zero);
    }

    public static int? ToIntegerOrDefault(this string integerValue)
    {
        return int.TryParse(integerValue, out var value) ? value : null;
    }
}