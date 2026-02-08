using System;

namespace NetCore8583.Extensions
{
    /// <summary>Enum parsing helpers (e.g. for configuration).</summary>
    public static class Enumm
    {
        /// <summary>Parses an enum value from a string (case-insensitive).</summary>
        /// <param name="name">The enum member name.</param>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <returns>The parsed value, or null if parsing fails.</returns>
        public static T? Parse<T>(string name) where T : struct
        {
            return (T) Enum.Parse(typeof(T),
                name,
                true);
        }
    }
}