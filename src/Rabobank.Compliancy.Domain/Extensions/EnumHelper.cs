namespace Rabobank.Compliancy.Domain.Extensions;

/// <summary>
/// A helper class that contains logic to handle enum conversions.
/// </summary>
public static class EnumHelper
{
    /// <summary>
    /// Converts the string representation of the name to an equivalent enumerated instance.
    /// </summary>
    /// <typeparam name="TEnum">The enum to be parsed.</typeparam>
    /// <param name="value">The value to be parsed to an <see cref="Enum"/> instance.</param>
    /// <returns>A default <see cref="Enum"/> instance.</returns>
    public static TEnum ParseEnumOrDefault<TEnum>(string value) where TEnum : struct
    {
        return Enum.TryParse(value, true, out TEnum result) ? result : default;
    }

    /// <summary>
    /// Converts the string representation of the name to an equivalent enumerated instance.
    /// </summary>
    /// <typeparam name="TEnum">The enum to be parsed.</typeparam>
    /// <param name="value">The value to be parsed to an <see cref="Enum"/> instance.</param>
    /// <returns>A nullable <see cref="Enum"/> instance.</returns>
    public static TEnum? ParseEnumOrNull<TEnum>(string value) where TEnum : struct
    {
        return Enum.TryParse(value, true, out TEnum result) ? result : null;
    }
}