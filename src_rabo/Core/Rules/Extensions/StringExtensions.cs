﻿namespace Rabobank.Compliancy.Core.Rules.Extensions;

public static class StringExtensions
{
    public static bool IsNotNullOrWhiteSpace(this string value)
    {
        return !IsNullOrWhiteSpace(value);
    }

    public static bool IsNullOrWhiteSpace(this string value)
    {
        return string.IsNullOrWhiteSpace(value);
    }
}