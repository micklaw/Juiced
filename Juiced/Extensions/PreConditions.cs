using System;

namespace Juiced
{
    internal static class PreConditions
    {
        public static T NotNull<T>(this T value, string key) where T : class
        {
            if (value == null)
            {
                throw new ArgumentNullException(key);
            }

            return value;
        }
    }
}