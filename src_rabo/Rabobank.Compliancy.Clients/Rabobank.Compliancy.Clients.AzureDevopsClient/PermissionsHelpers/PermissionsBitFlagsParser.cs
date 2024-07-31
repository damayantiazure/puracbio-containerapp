namespace Rabobank.Compliancy.Infrastructure.Parsers;

public static class PermissionsBitFlagsParser
{
    public static bool IsEnumFlagPresent<TEnum>(int? value, TEnum lookingForFlag)
        where TEnum : struct
    {
        if (value == null)
        {
            return false;
        }

        int castedLookingForFlag = (int)(object)lookingForFlag;
        return (value & castedLookingForFlag) == castedLookingForFlag;
    }
}