using System;

namespace Core.Extensions
{
    public static class PrimitiveTypeExtensions
    {
        public static Guid AsGuidOrDefault(this string @this)
        {
            if (string.IsNullOrWhiteSpace(@this))
            {
                return default;
            }
            return Guid.TryParse(@this, out var guid) ? guid : default;
        }

        public static string AsStringOrDefault(this Guid @this)
        {
            if (@this == default)
            {
                return default;
            }
            return @this.ToString();
        }
    }
}
