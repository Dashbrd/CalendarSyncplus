using System;

namespace CalendarSyncPlus.Common
{
    public static class StringExtensions
    {
        public static TEnum ToEnum<TEnum>(this string enumValue,bool ignoreCase=false) where TEnum : struct
        {
            TEnum @enum;
            return Enum.TryParse(enumValue, ignoreCase, out @enum) ? @enum : new TEnum();
        }
    }
}